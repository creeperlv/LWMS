using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CLUNL.Pipeline;
using LWMS.Core.Authentication;
using LWMS.Core.WR;
using LWMS.Management;

namespace LWMS.Core.RemoteShell.Server
{
    public class RSServer
    {
        public int MaxConnections { get => _MaxConnections; }
        int _MaxConnections;
        //BufferManager
        //SocketAsyncEventArgsPool eventArgsPool;
        internal static byte[] RSAPublicKey;
        internal static byte[] RSAPrivateKey;
        public Socket Listener;
        Semaphore ThreadLimitation;
        RSA rsa = RSA.Create();
        public static void SetPublicKey(byte[] key, string auth)
        {
            OperatorAuthentication.AuthedAction(auth, () => { RSAPublicKey = key; }, false, false, PermissionID.RS_SetPubKey, PermissionID.RS_All);
        }
        public static void SetPrivateKey(byte[] key, string auth)
        {
            OperatorAuthentication.AuthedAction(auth, () => { RSAPrivateKey = key; }, false, false, PermissionID.RS_SetPriKey, PermissionID.RS_All);
        }
        public RSServer(IPEndPoint point, int MaxConnections)
        {
            Listener = new Socket(point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(point);
            this._MaxConnections = MaxConnections;
            ThreadLimitation = new Semaphore(MaxConnections, MaxConnections);
        }
        public static void SetFunctions(Func<string, List<CommandPack>> ResolveCommand, Action<string, CommandPack[]> Control,string Auth)
        {
            OperatorAuthentication.AuthedAction(Auth, () => {
                RSServer.ResolveCommand = ResolveCommand;
                RSServer.Control = Control;
            }, false, false, PermissionID.RS_SetFunctions, PermissionID.RS_All);
        }
        bool Stop = false;
        public void Start()
        {
            rsa.ImportRSAPrivateKey(RSAPrivateKey, out _);
            rsa.ImportRSAPublicKey(RSAPublicKey, out _);
            byte[] b = BitConverter.GetBytes(100);
            Listener.Listen(100);
            Task.Run(() =>
            {
                while (Stop is false)
                {
                    var s = Listener.Accept();
                    var v = ValidAndLogin(s);
                    if (v.Item1)
                    {
                        ThreadLimitation.WaitOne();
                        AESLayer _s = v.Item2;
                        RSCMDReciver.AddSocket(_s);
                        Watch(_s);
                    }
                    else s.Close();
                }
            });
        }
        static Func<string, List<CommandPack>> ResolveCommand;
        static Action<string, CommandPack[]> Control;
        internal void Watch(AESLayer s)
        {
            s.client.ReceiveTimeout = 0;
            _ = Task.Run(() =>
              {
                  while (s.isDisposed is false)
                  {
                      try
                      {
                          s.Read(out byte[] d);
                          int Operatation = BitConverter.ToInt32(d);
                          switch (Operatation)
                          {
                              case 1:
                                  {
                                      // Receieve Command.
                                      s.Read(out byte[] c);
                                      s.Read(out byte[] a);
                                      string command = Encoding.UTF8.GetString(c);
                                      string auth = Encoding.UTF8.GetString(a);
                                      var cmdList = ResolveCommand(command);
                                      Control(auth, cmdList.ToArray());
                                  }
                                  break;
                              default:
                                  {

                                      byte[] Refuse = new byte[] { (byte)'?', (byte)'?', (byte)'?' };
                                      s.Write(Refuse);
                                  }
                                  break;
                          }
                      }
                      catch (Exception)
                      {
                          s.Dispose();
                      }
                  }
              });
        }
        internal (bool, AESLayer) ValidAndLogin(Socket client)
        {
            client.ReceiveTimeout = 30*1000;//Set time-out to 30 seconds.
            client.Send(BitConverter.GetBytes(RSAPublicKey.Length));
            client.Send(RSAPublicKey);
            byte[] Key;
            byte[] IV;
            AESLayer layer;
            {
                byte[] d0 = new byte[256];
                client.Receive(d0, 256, SocketFlags.None);
                var D0 = rsa.Decrypt(d0, RSAEncryptionPadding.OaepSHA256);
                d0 = new byte[256];
                client.Receive(d0, 256, SocketFlags.None);
                var D1 = rsa.Decrypt(d0, RSAEncryptionPadding.OaepSHA256);
                d0 = new byte[256];
                client.Receive(d0, 256, SocketFlags.None);
                IV = rsa.Decrypt(d0, RSAEncryptionPadding.OaepSHA256);
                List<byte> d = new List<byte>(D0);
                d.AddRange(D1);
                Key = d.ToArray();
                layer = new AESLayer(Key, IV, client);
                {
                    byte[] NameD;
                    byte[] KeyD;
                    layer.Read(out NameD);
                    layer.Read(out KeyD);
                    string UName = Encoding.UTF8.GetString(NameD);
                    string UKey = Encoding.UTF8.GetString(KeyD);
                    var auth = OperatorAuthentication.ObtainRTAuth(UName, UKey);
                    if (OperatorAuthentication.IsAuthPresent(auth))
                    {
                        layer.Write(Encoding.UTF8.GetBytes(auth));
                        layer.Auth = auth;
                        return (true, layer);
                    }
                    else
                    {
                        byte[] Refuse = new byte[] { (byte)'N', (byte)'O' };
                        layer.Write(Refuse);
                        return (false, null);
                    }
                }
            }
        }
    }
    class AESLayer : IDisposable
    {
        internal bool isDisposed = false;
        Aes aes = Aes.Create();
        internal Socket client;
        internal string Auth;
        ICryptoTransform Decryptor;
        ICryptoTransform Encryptor;
        internal AESLayer(byte[] AESKey, byte[] AESIV, Socket client)
        {
            this.client = client;
            aes.Key = AESKey;
            aes.IV = AESIV;
            Decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            Encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        }
        public void Write(byte[] data)
        {
            if (client.Connected == false)
            {
                Dispose();
            }
            {
                byte[] length = BitConverter.GetBytes(data.Length);
                var _data = PaddingByte(length, 16);
                byte[] final = new byte[_data.Length];
                Encryptor.TransformBlock(_data, 0, _data.Length, final, 0);
                client.Send(final);
            }
            {
                var _data = PaddingByte(data, 16);
                byte[] final = new byte[_data.Length];
                Encryptor.TransformBlock(_data, 0, _data.Length, final, 0);
                client.Send(final);
            }
        }
        public void Read(out byte[] data)
        {
            int length;
            {
                byte[] rD = new byte[16];
                byte[] d = new byte[16];
                client.Receive(rD, 16, SocketFlags.None);
                Decryptor.TransformBlock(rD, 0, 16, d, 0);
                length = BitConverter.ToInt32(new byte[] { d[0], d[1], d[2], d[3] });
            }
            {
                int Padded = ((length / 16) + 1) * 16;
                byte[] rD = new byte[Padded];
                byte[] d = new byte[Padded];
                client.Receive(rD, Padded, SocketFlags.None);
                Decryptor.TransformBlock(rD, 0, Padded, d, 0);
                data = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    data[i] = d[i];
                }
            }
        }
        byte[] PaddingByte(byte[] OriginalData, int Size)
        {
            if (OriginalData.Length % Size == 0) return OriginalData;
            else
            {
                int NewSize = ((OriginalData.Length / Size) + 1) * Size;
                byte[] b = new byte[NewSize];
                b.Initialize();
                OriginalData.CopyTo(b, 0);
                return b;
            }
        }

        public void Dispose()
        {
            isDisposed = true;
            aes.Dispose();
            client.Dispose();
        }
    }
    public class RSCMDReciver : IPipedProcessUnit
    {
        static List<AESLayer> layers = new List<AESLayer>();
        internal static void AddSocket(AESLayer s)
        {
            layers.Add(s);
        }
        public PipelineData Process(PipelineData Input)
        {
            {
                PipedRoutedWROption option = (PipedRoutedWROption)Input.Options;
                if (option.PipedRoutedWROperation == PipedRoutedWROperation.WRITE)
                {
                    for (int i = 0; i < layers.Count; i++)
                    {
                        var item = layers[i];
                        if (item.isDisposed == true)
                        {
                            layers.Remove(item);
                        }
                        else
                        {
                            if (item.Auth == option.AuthContext)
                            {
                                item.Write(BitConverter.GetBytes(0));
                                item.Write(Encoding.UTF8.GetBytes((string)Input.PrimaryData));
                            }
                        }
                    }
                }
                else if (option.PipedRoutedWROperation == PipedRoutedWROperation.WRITELINE)
                {
                    for (int i = 0; i < layers.Count; i++)
                    {
                        var item = layers[i];
                        if (item.isDisposed == true)
                        {
                            layers.Remove(item);
                        }
                        else
                        {
                            if (item.Auth == option.AuthContext)
                            {
                                item.Write(BitConverter.GetBytes(1));
                                item.Write(Encoding.UTF8.GetBytes((string)Input.PrimaryData));
                            }
                        }
                    }
                }
                else if (option.PipedRoutedWROperation == PipedRoutedWROperation.FLUSH)
                {
                    for (int i = 0; i < layers.Count; i++)
                    {
                        var item = layers[i];
                        if (item.isDisposed == true)
                        {
                            layers.Remove(item);
                        }
                        else
                        {
                            if (item.Auth == option.AuthContext)
                            {
                                item.Write(BitConverter.GetBytes(2));
                                item.Write(BitConverter.GetBytes(-1));
                            }
                        }
                    }
                }
                else
                {
                    if (option.PipedRoutedWROperation == PipedRoutedWROperation.FGCOLOR)
                    {
                        for (int i = 0; i < layers.Count; i++)
                        {
                            var item = layers[i];
                            if (item.isDisposed == true)
                            {
                                layers.Remove(item);
                            }
                            else
                            {
                                if (item.Auth == option.AuthContext)
                                {
                                    item.Write(BitConverter.GetBytes(3));
                                    item.Write(Encoding.UTF8.GetBytes(((ConsoleColor)Input.PrimaryData).ToString()));
                                }
                            }
                        }
                    }
                    else if (option.PipedRoutedWROperation == PipedRoutedWROperation.BGCOLOR)
                    {
                        for (int i = 0; i < layers.Count; i++)
                        {
                            var item = layers[i];
                            if (item.isDisposed == true)
                            {
                                layers.Remove(item);
                            }
                            else
                            {
                                if (item.Auth == option.AuthContext)
                                {
                                    item.Write(BitConverter.GetBytes(4));
                                    item.Write(Encoding.UTF8.GetBytes(((ConsoleColor)Input.PrimaryData).ToString()));
                                }
                            }
                        }
                    }
                    else if (option.PipedRoutedWROperation == PipedRoutedWROperation.RESETCOLOR)
                    {
                        for (int i = 0; i < layers.Count; i++)
                        {
                            var item = layers[i];
                            if (item.isDisposed == true)
                            {
                                layers.Remove(item);
                            }
                            else
                            {
                                if (item.Auth == option.AuthContext)
                                {
                                    item.Write(BitConverter.GetBytes(5));
                                    item.Write(BitConverter.GetBytes(-1));
                                }
                            }
                        }
                    }
                }
            }
            return Input;
        }
    }
}

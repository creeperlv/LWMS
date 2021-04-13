using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        internal static byte[] RSAPublicKey = null;
        internal static byte[] RSAPrivateKey = null;
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
        public static void SetFunctions(Func<string, List<CommandPack>> ResolveCommand, Action<string, CommandPack[]> Control, string Auth)
        {
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                RSServer.ResolveCommand = ResolveCommand;
                RSServer.Control = Control;
            }, false, false, PermissionID.RS_SetFunctions, PermissionID.RS_All);
        }
        bool Stop = false;
        public void Start()
        {
            Trace.WriteLine("Remote Shell Server inited.");
            if (RSAPrivateKey is not null)
                rsa.ImportRSAPrivateKey(RSAPrivateKey, out _);
            else RSAPrivateKey = rsa.ExportRSAPrivateKey();
            if (RSAPublicKey is not null)
                rsa.ImportRSAPublicKey(RSAPublicKey, out _);
            else RSAPublicKey = rsa.ExportRSAPublicKey();
            Listener.Listen(100);
            Task.Run(() =>
            {
                while (Stop is false)
                {

                    var s = Listener.Accept();
                    Trace.WriteLine("New Remote Connection Request:" + s.RemoteEndPoint.ToString());
                        ThreadLimitation.WaitOne();
                        Watch(s);
                }
            });
        }
        static Func<string, List<CommandPack>> ResolveCommand;
        static Action<string, CommandPack[]> Control;
        internal void Watch(Socket s)
        {

            var v = ValidAndLogin(s);

            AESLayer _s;
            if (v.Item1)
            {
                _s = v.Item2;
                RSCMDReciver.AddSocket(_s);
            }
            else { s.Close();return; }
            _s.client.ReceiveTimeout = 0;
            _ = Task.Run(() =>
              {
                  Trace.WriteLine("Conntection Logged in:"+ _s.client.RemoteEndPoint.ToString());
                  while (_s.isDisposed is false)
                  {
                      try
                      {
                          _s.Read(out byte[] d);
                          int Operatation = BitConverter.ToInt32(d);
                          switch (Operatation)
                          {
                              case 1:
                                  {
                                      // Receieve Command.
                                      _s.Read(out byte[] c);
                                      _s.Read(out byte[] a);
                                      string command = Encoding.UTF8.GetString(c);
                                      string auth = Encoding.UTF8.GetString(a);
                                      var cmdList = ResolveCommand(command);
                                      Control(auth, cmdList.ToArray());
                                  }
                                  break;
                              default:
                                  {

                                      byte[] Refuse = new byte[] { (byte)'?', (byte)'?', (byte)'?' };
                                      _s.Write(Refuse);
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
        static string FingerPrint(byte[] data)
        {
            var d = SHA256.HashData(data);
            string str = "";

            for (int i = 0; i < d.Length; i++)
            {
                var item = d[i];
                str += string.Format("{0:x2}", item) + " ";
                if (i == (d.Length / 2) - 1)
                {
                    str += "\r\n";
                }
            }
            return str.ToUpper();
        }
        internal (bool, AESLayer) ValidAndLogin(Socket client)
        {
            client.ReceiveTimeout = 30 * 1000;//Set time-out to 30 seconds.
            client.Send(BitConverter.GetBytes(RSAPublicKey.Length));
            client.Send(RSAPublicKey);

            byte[] Key;
            byte[] IV;
            AESLayer layer;
            {
                byte[] d0 = new byte[256];
                client.Receive(d0, 256, SocketFlags.None);
                Key = rsa.Decrypt(d0, RSAEncryptionPadding.OaepSHA256);
                d0 = new byte[256];
                client.Receive(d0, 256, SocketFlags.None);
                IV = rsa.Decrypt(d0, RSAEncryptionPadding.OaepSHA256);
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
            client.ReceiveBufferSize = 16;
            client.SendBufferSize = 16;
            aes.Key = AESKey;
            aes.IV = AESIV;
            aes.Padding = PaddingMode.None;
            Encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            Decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        }
        public void Write(byte[] data)
        {
            if (client.Connected == false)
            {
                Dispose();
            }
            {
                byte[] length = BitConverter.GetBytes(-data.Length);
                var _data = PaddingByte(length, 16);
                byte[] final;
                final=Encryptor.TransformFinalBlock(_data, 0, _data.Length);
                client.Send(final);
            }
            {
                var _data = PaddingByte(data, 16);
                byte[] final;
                final = Encryptor.TransformFinalBlock(_data, 0, _data.Length);
                client.Send(final);

            }
        }
        public void Read(out byte[] data)
        {
            int length;
            {
                byte[] rD = new byte[16];
                byte[] d = new byte[16];
                client.Receive(rD, 0, 16, SocketFlags.None);
               
                d = Decryptor.TransformFinalBlock(rD, 0, 16);
               
                length = -BitConverter.ToInt32(new byte[] { d[0], d[1], d[2], d[3] });
            }
            {
                int Padded = ((length / 16) + 1) * 16;
                byte[] rD = new byte[Padded];
                byte[] d = new byte[Padded];
                client.Receive(rD, Padded, SocketFlags.None);
                d=Decryptor.TransformFinalBlock(rD, 0, Padded);
                data = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    data[i] = d[i];
                }
            }
        }
        byte[] PaddingByte(byte[] OriginalData, int Size)
        {
            if (OriginalData.Length == 0)
            {
                var b = new byte[Size];
                for (int i = 0; i < Size; i++)
                {
                    b[i] = 0;
                }
                return b;
            }
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
                            //Console.WriteLine($"Is {item.Auth } equals to {option.AuthContext}?");
                            if (item.Auth == option.AuthContext)
                            {
                                //Console.WriteLine("Wrote Line.");
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

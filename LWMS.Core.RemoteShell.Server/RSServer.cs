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

namespace LWMS.Core.RemoteShell.Server
{
    public class RSServer
    {
        int MaxConnections;
        int BufferSize;
        //BufferManager
        //SocketAsyncEventArgsPool eventArgsPool;
        public static byte[] RSAPublicKey;
        public static byte[] RSAPrivateKey;
        public Socket Listener;
        Semaphore ThreadLimitation;
        RSA rsa = RSA.Create();
        public RSServer(IPEndPoint point, int MaxConnections, int BufferSize)
        {
            Listener = new Socket(point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(point);
            this.MaxConnections = MaxConnections;
            this.BufferSize = BufferSize;
            ThreadLimitation = new Semaphore(MaxConnections, MaxConnections);
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
                    }
                    else s.Close();
                }
            });
        }
        internal void Watch((string, Socket) s)
        {
            s.Item2.ReceiveTimeout = 0;
        }
        internal (bool, AESLayer) ValidAndLogin(Socket client)
        {
            client.Send(RSAPublicKey);
            byte[] Key = null;
            byte[] IV = null;
            AESLayer layer;
            {
                byte[] d0 = new byte[256];
                client.Receive(d0, 256, SocketFlags.None);
                var D0 = rsa.Decrypt(d0, RSAEncryptionPadding.OaepSHA256);
                d0 = new byte[256];
                client.Receive(Key, 256, SocketFlags.None);
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
                    var auth=OperatorAuthentication.ObtainRTAuth(UName, UKey);
                    if (OperatorAuthentication.IsAuthPresent(auth))
                    {
                        layer.Write(Encoding.UTF8.GetBytes(auth));
                        return (true, layer);
                    }
                    else {return (false, null); }
                }
            }
            return (false, null);
        }
    }
    class AESLayer:IDisposable
    {
        Aes aes = Aes.Create();
        Socket client;
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
            aes.Dispose();
            client.Dispose();
        }
    }
    public class RSCMDReciver : IPipedProcessUnit
    {
        internal static void AddSocket(AESLayer s)
        {

        }
        public PipelineData Process(PipelineData Input)
        {
            {
                PipedRoutedWROption option = (PipedRoutedWROption)Input.Options;
                if (option.PipedRoutedWROperation == PipedRoutedWROperation.WRITE)
                {
                    Console.Out.Write((string)Input.PrimaryData);
                }
                else if (option.PipedRoutedWROperation == PipedRoutedWROperation.WRITELINE)
                {
                    Console.Out.WriteLine((string)Input.PrimaryData);
                }
                else if (option.PipedRoutedWROperation == PipedRoutedWROperation.FLUSH)
                {
                    Console.Out.Flush();
                }
                else
                {
                    if (option.PipedRoutedWROperation == PipedRoutedWROperation.FGCOLOR)
                    {
                        Console.ForegroundColor = (ConsoleColor)Input.PrimaryData;
                    }
                    else if (option.PipedRoutedWROperation == PipedRoutedWROperation.BGCOLOR)
                    {
                        Console.BackgroundColor = (ConsoleColor)Input.PrimaryData;
                    }
                    else if (option.PipedRoutedWROperation == PipedRoutedWROperation.RESETCOLOR)
                    {
                        Console.ResetColor();
                    }
                }
            }
            return Input;
        }
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LWMS.Core.RemoteShell.ClientCore
{
    public class RSClient
    {
        IPEndPoint target;
        public RSClient(IPEndPoint target)
        {
            this.target = target;
        }
        Socket s; byte[] RSAPubKey;
        public byte[] Handshake00()
        {
            s = new Socket(target.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            s.Connect(target);
            int length = 1024;
            byte[] KeyLength = new byte[4];
            s.Receive(KeyLength, 4, SocketFlags.None);
            length = BitConverter.ToInt32(KeyLength);
            RSAPubKey = new byte[length];
            s.Receive(RSAPubKey, length, SocketFlags.None);
            return RSAPubKey;
        }
        AESLayer layer;
        internal string Auth;
        public bool Handshake01(string UserName, string Password)
        {
            var aes = Aes.Create(); aes.GenerateKey(); aes.GenerateIV();
            layer = new AESLayer(aes.Key, aes.IV, s);
            var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(RSAPubKey, out _);
            {
                //Send AES Key
                byte[] Part1 = new byte[190];
                for (int i = 0; i < 190; i++)
                {
                    Part1[i] = aes.Key[i];
                }
                var FData0 = rsa.Encrypt(Part1, RSAEncryptionPadding.OaepSHA256);
                s.Send(FData0, 256, SocketFlags.None);
            }
            {
                //Send AES Key
                byte[] Part2 = new byte[66];
                for (int i = 0; i < 66; i++)
                {
                    Part2[i] = aes.Key[i + 190];
                }
                var FData1 = rsa.Encrypt(Part2, RSAEncryptionPadding.OaepSHA256);
                s.Send(FData1, 256, SocketFlags.None);
            }
            {
                //Send AES IV
                var FinalEncIV = rsa.Encrypt(aes.IV, RSAEncryptionPadding.OaepSHA256);
                s.Send(FinalEncIV, 256, SocketFlags.None);
            }
            {
                //Send U/N
                layer.Write(Encoding.UTF8.GetBytes(UserName));
                layer.Write(Encoding.UTF8.GetBytes(Password));
            }
            {
                byte []receieved=null;
                layer.Read(out receieved);
                if (receieved.Length >= 2)
                {
                    if (receieved.Length is 2 && receieved[0] == 'N' && receieved[1] == 'O')
                    {
                        return false;
                    }
                    else
                    {
                        Auth = Encoding.UTF8.GetString(receieved);
                        return true;
                    }
                }
                else return false;
            }
        }
        public void SendOut(string cmd)
        {
            layer.Write(Encoding.UTF8.GetBytes(cmd));
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
}

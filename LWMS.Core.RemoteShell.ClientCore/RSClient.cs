using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LWMS.Core.RemoteShell.ClientCore
{
    public class RSClient : IDisposable
    {
        IPEndPoint target;
        public RSClient(IPEndPoint target)
        {
            this.target = target;
        }
        Socket s = null; byte[] RSAPubKey;
        List<IOutput> Outputs = new List<IOutput>();
        public void RegisterOutput(IOutput output)
        {
            Outputs.Add(output);
        }
        public void UnregisterOutput(IOutput output)
        {
            if (Outputs.Contains(output))
            {
                Outputs.Remove(output);
            }
        }
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
        bool Stop = false;
        void Listen()
        {
            try
            {
                while (Stop is not true)
                {
                    int Operation = -1;
                    {
                        layer.Read(out byte[] d);
                        Operation = BitConverter.ToInt32(d);
                    }
                    {
                        switch (Operation)
                        {
                            case 0:
                                {
                                    layer.Read(out byte[] d);
                                    string msg = Encoding.UTF8.GetString(d);
                                    foreach (var item in Outputs)
                                    {
                                        item.Write(msg);
                                    }
                                }
                                break;
                            case 1:
                                {
                                    layer.Read(out byte[] d);
                                    string msg = Encoding.UTF8.GetString(d);
                                    foreach (var item in Outputs)
                                    {
                                        item.WriteLine(msg);
                                    }
                                }
                                break;
                            case 2:
                                {
                                    layer.Read(out _);
                                    foreach (var item in Outputs)
                                    {
                                        item.Flush();
                                    }
                                }
                                break;
                            case 3:
                                {
                                    try
                                    {
                                        layer.Read(out byte[] d);
                                        string msg = Encoding.UTF8.GetString(d);
                                        var color = Enum.Parse<ConsoleColor>(msg);
                                        foreach (var item in Outputs)
                                        {
                                            item.SetForeground(color);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                                break;
                            case 4:
                                {
                                    try
                                    {
                                        layer.Read(out byte[] d);
                                        string msg = Encoding.UTF8.GetString(d);
                                        var color = Enum.Parse<ConsoleColor>(msg);
                                        foreach (var item in Outputs)
                                        {
                                            item.SetBackground(color);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                                break;
                            case 5:
                                {
                                    layer.Read(out _);
                                    foreach (var item in Outputs)
                                    {
                                        item.ResetColor();
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        public bool Handshake01(string UserName, string Password)
        {
            var aes = Aes.Create(); aes.GenerateKey(); aes.GenerateIV();
            layer = new AESLayer(aes.Key, aes.IV, s);
            var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(RSAPubKey, out _);
            {
                //Send AES Key
                //byte[] Part1 = new byte[190];
                //for (int i = 0; i < 32; i++)
                //{
                //    Part1[i] = aes.Key[i];
                //}
                var FData0 = rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA256);
                s.Send(FData0, 256, SocketFlags.None);
            }
            //{
            //    //Send AES Key
            //    byte[] Part2 = new byte[66];
            //    for (int i = 0; i < 66; i++)
            //    {
            //        Part2[i] = aes.Key[i + 190];
            //    }
            //    var FData1 = rsa.Encrypt(Part2, RSAEncryptionPadding.OaepSHA256);
            //    s.Send(FData1, 256, SocketFlags.None);
            //}
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
                byte[] receieved = null;
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
                        Task.Run(Listen);
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

        public void Dispose()
        {
            Stop = true;
            if (s is not null) s.Dispose();
            Outputs.Clear();
            Outputs = null;
        }
    }
    public interface IOutput
    {
        void WriteLine(string msg);
        void Write(string msg);
        void SetForeground(ConsoleColor color);
        void SetBackground(ConsoleColor color);
        void ResetColor();
        void Flush();
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
                Encryptor.TransformBlock(_data,0, _data.Length, final,0);

                //Decryptor.TransformBlock(_data,0, _data.Length, final,0);
                client.Send(final,final.Length, SocketFlags.None);
            }
            {
                var _data = PaddingByte(data, 16);
                byte[] final = new byte[_data.Length];
                Encryptor.TransformBlock(_data, 0, _data.Length, final, 0);
                client.Send(final, final.Length, SocketFlags.None);
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
                for (int i = 0; i < OriginalData.Length; i++)
                {
                    b[i] = OriginalData[i];

                }
                //OriginalData.CopyTo(b, 0);
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

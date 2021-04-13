using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LWMS.Core.RemoteShell.ClientCore;

namespace LWMS.RemoteShell.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Copyright (C) 2020-2021 Creeper Lv");
            Console.WriteLine("This software is licensed under the MIT License");
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Console.WriteLine(path);
            var Conf = Path.Combine(path, "Settings.ini");
            SimpleConfig config = null;
            if (File.Exists(Conf))
            {
                config = SimpleConfig.LoadFromFile(Conf);
            }
            var conf = Path.Combine(Environment.CurrentDirectory, "Settings.ini");
            if (File.Exists(conf))
            {
                config = SimpleConfig.LoadFromFile(conf);

            }
            IPEndPoint endPoint;
            string Address = null;
            int port = -1;
            string AcceptedPubKey = null;
            if (config is not null)
            {
                Address = config.GetValue("ServerAddress", null);
                {
                    _ = int.TryParse(config.GetValue("ServerPort", null), out port);
                }
                {
                    AcceptedPubKey = config.GetValue("ServerPubKey", null);
                }
            }
            if (Address is null)
            {
                Console.WriteLine("Please enter a target server:");
                Address = Console.ReadLine();
            }
            if (port is -1)
            {
                Console.WriteLine("Please specify which port to connect:(22 by default)");

                if (int.TryParse(Console.ReadLine(), out port) == false) port = 22;
            }

            endPoint = new IPEndPoint(Dns.GetHostAddresses(Address).First(), port);
            RSClient client = new RSClient(endPoint);
            client.RegisterOutput(new ConsoleOut());
            client.RegisterOnConnectionLost(() => {
                Console.WriteLine("Connection Lost.");
            });
            var pubKey = client.Handshake00();
            Console.WriteLine("Received public key from server:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(FingerPrint(pubKey));
            Console.ResetColor();
            Console.WriteLine("Please make sure if the key is from server.");
            bool isAccepted = false;
            if (AcceptedPubKey is not null)
            {
                Console.WriteLine("The key is automatically accpeted.");
                if (Convert.ToBase64String(pubKey) == AcceptedPubKey) isAccepted = true;
            }
            else
            {
                Console.WriteLine("Enter \"Yes\" or \"Y\" to accept the key.");
                var r = Console.ReadLine().ToUpper();
                isAccepted = r == "YES" || r == "Y";
            }

            if (isAccepted == true)
            {
                Console.WriteLine("Please enter your user name:");
                var un = Console.ReadLine();
                Console.WriteLine("Please enter your password:");
                var pw = ReadPassword();
                var r = client.Handshake01(un, pw);
                if (r == true)
                {
                    Console.WriteLine("Logged in.");
                    while (true)
                    {
                        var cmd = Console.ReadLine();
                        if (cmd.ToUpper() == "EXIT" || cmd.ToUpper() == "/Q" || cmd.ToUpper() == "QUIT")
                        {
                            Environment.Exit(0);
                        }else if(cmd.ToUpper() == "CLS"|| cmd.ToUpper() == "CLEAR")
                        {
                            Console.Clear();
                        }
                        else
                        {
                            client.SendOut(cmd);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Rejected by the server.");
                }
            }
            static string ReadPassword()
            {
                StringBuilder password = new StringBuilder();
                ConsoleKeyInfo con;

                do
                {
                    con = Console.ReadKey(true);
                    if (con.Key == ConsoleKey.Backspace)
                    {
                        try
                        {
                            password.Remove(password.Length - 1, 1);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        if (con.Key is >= ConsoleKey.D0 and <= ConsoleKey.Z)
                        {
                            password.Append(con.KeyChar.ToString());
                        }
                    }
                } while (con.Key != ConsoleKey.Enter);
                return password.ToString();
            }
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
    }
    class ConsoleOut : IOutput
    {
        public void Flush()
        {
            Console.Out.Flush();
        }

        public void ResetColor()
        {
            Console.ResetColor();
        }

        public void SetBackground(ConsoleColor color)
        {
            Console.BackgroundColor = color;
        }

        public void SetForeground(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        public void Write(string msg)
        {
            Console.Write(msg);
        }

        public void WriteLine(string msg)
        {
            Console.WriteLine(msg);
        }
    }
    class SimpleConfig
    {
        Dictionary<string, string> CoreData = new();
        internal string GetValue(string key, string fallback = "")
        {
            if (CoreData.ContainsKey(key))
            {
                return CoreData[key];
            }
            else return fallback;
        }
        internal static SimpleConfig LoadFromFile(string FileName)
        {
            SimpleConfig simpleConfig = new SimpleConfig();
            var lines = File.ReadAllLines(FileName);
            foreach (var item in lines)
            {
                var d = item.Trim();
                if (d.StartsWith("#"))
                {
                    //Ignore
                }
                else
                {
                    if (d.IndexOf("=") > 0)
                    {
                        var k = d.Substring(0, d.IndexOf('='));
                        var v = d.Substring(d.IndexOf('=') + 1);
                        if (simpleConfig.CoreData.ContainsKey(k))
                            simpleConfig.CoreData[k] = v;
                        else simpleConfig.CoreData.Add(k, v);
                    }
                    else
                    {
                        Console.WriteLine("W: Line:" + item + " in configuration file is not understandable.");
                        //Ignore
                    }
                }
            }
            return simpleConfig;
        }
    }

}

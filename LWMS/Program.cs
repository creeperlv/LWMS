using LWMS.Core;
using LWMS.Core.Authentication;
using LWMS.Core.Configuration;
using LWMS.Core.Log;
using LWMS.Core.Utilities;
using LWMS.Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace LWMS
{
    class Program
    {
        static string Auth;
        static void Main(string[] args)
        {
            {
                var Auth0 = CLUNL.Utilities.RandomTool.GetRandomString(32, CLUNL.Utilities.RandomStringRange.R3);
                var Auth1 = CLUNL.Utilities.RandomTool.GetRandomString(32, CLUNL.Utilities.RandomStringRange.R3);
                Auth = OperatorAuthentication.ObtainRTAuth(Auth0, Auth1);
                OperatorAuthentication.SetLocalHostAuth(Auth);
            }
            Console.WriteLine("Copyright (C) 2020 Creeper Lv");
            Console.WriteLine("This software is licensed under the MIT License");
            var _commands = Tools00.ResolveCommand(Environment.CommandLine);
            _commands.RemoveAt(0);//Remove start command.
            if (_commands.Count > 0)
            {
                if (_commands[0].PackTotal.ToUpper() == "/PREBOOT")
                {
                    _commands.RemoveAt(0);
                    LWMSCoreServer.LoadCommandsFromManifest();
                    ServerController.Control(Auth, _commands.ToArray());
                }
                else if (_commands[0].PackTotal.ToUpper() == "/NOBOOT")
                {
                    _commands.RemoveAt(0);
                    LWMSCoreServer.LoadCommandsFromManifest();
                    ServerController.Control(Auth, _commands.ToArray());
                    return;
                }
                else { }

            }
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            LWMSTraceListener.BeautifyConsoleOutput = true;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("This software is in active development and extremely unstable, do not use it in production environment!");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("LWMS - LightWeight Managed Server");
            Console.WriteLine();
            Check00();
            LWMSCoreServer coreServer = new LWMSCoreServer();
            //coreServer.Bind("http://+:8080/");
            //string p = Configuration.BasePath;
            coreServer.Start(100);
            Console.WriteLine("The server is now running good.");
            Login();
            if (args.Length > 0)
            {
                var cmd = new List<CommandPack>();
                foreach (var item in _commands)
                {
                    cmd.Add(item);
                }
                ServerController.Control(Auth, cmd.ToArray());
            }
            CommandListener();
        }
        static void Login()
        {
            while (true)
            {

                Console.WriteLine("User Name:");
                var UN = Console.ReadLine();
                Console.WriteLine("Password:");
                var PW = ReadPassword();
                if (OperatorAuthentication.IsAuthPresent(OperatorAuthentication.ObtainRTAuth(UN, PW)) is true){
                    Auth = OperatorAuthentication.ObtainRTAuth(UN, PW);
                    Console.WriteLine("Welcome, "+UN);
                    return;
                }
                Console.WriteLine("Username + Password combination no found, please retry.");
            }
        }
        static void CommandListener()
        {
            //PrintHint();
            while (true)
            {
                var cmd = Console.ReadLine();
                if (cmd == "") continue;//Skip blank line.
                var cmdList = Tools00.ResolveCommand(cmd);
                ServerController.Control(Auth, cmdList.ToArray());
                //PrintHint();
            }
        }
        //static void PrintHint()
        //{
        //    Console.Write("[");
        //    Console.ForegroundColor = ConsoleColor.Green;
        //    Console.Write("Local");
        //    Console.ResetColor();
        //    Console.Write("]>");
        //}
        static void Check00()
        {
            if (GlobalConfiguration.ListenPrefixes.Count == 0)
            {
                Console.WriteLine("Listening Url Not Found!");
                Console.WriteLine("Enter your own url prefixes: (End with \"END\")");
                string URL = null;
                List<string> RecordedUrls = new List<string>();
                while ((URL = Console.ReadLine().Trim()).ToUpper() != "END")
                {
                    if (URL.ToUpper() == "UNDO") RecordedUrls.RemoveAt(RecordedUrls.Count - 1);
                    RecordedUrls.Add(URL);
                }
                GlobalConfiguration.ListenPrefixes = RecordedUrls;
                Console.WriteLine("Done!");
            }
            if (OperatorAuthentication.HasAdmin() is not true)
            {
                Console.WriteLine("Create an administrator::");
                Console.WriteLine("User Name:");
                string Name = Console.ReadLine();
                bool isSucceed = false;
                while (isSucceed is not true)
                {
                    Console.WriteLine("Enter password for the first time:");
                    var pw0 = ReadPassword();
                    Console.WriteLine("Please repeat your password:");
                    var pw1 = ReadPassword();
                    if (pw0 == pw1)
                    {
                        var ID=OperatorAuthentication.CreateAuth(Auth, Name, pw0);
                        OperatorAuthentication.SetPermission(Auth, ID, "Class1Admin", true);
                        Auth = OperatorAuthentication.ObtainRTAuth(Name, pw0);
                        Console.WriteLine("Succeed.");
                        isSucceed = true;
                    }
                }
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
}

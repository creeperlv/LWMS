using LWMS.Core;
using LWMS.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace LWMS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Copyright (C) 2020 Creeper Lv");
            Console.WriteLine("This software is licensed under the MIT License");
            var _commands = Tools00.ResolveCommand(Environment.CommandLine);
            _commands.RemoveAt(0);//Remove start command.
            if (_commands.Count > 0)
            {
                if (_commands[0].PackTotal.ToUpper() == "/PREBOOT")
                {
                    _commands.RemoveAt(0);
                    ServerController.Control(_commands.ToArray());
                }
                else if (_commands[0].PackTotal.ToUpper() == "/NOBOOT")
                {
                    _commands.RemoveAt(0);
                    ServerController.Control(_commands.ToArray());
                    return;
                }
                else { }

            }
            LWMSTraceListener.BeautifyConsoleOutput = true;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("This software is in active development and extremely unstable, do not use it in production environment!");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("LWMS - LightWeight Managed Server");
            Console.WriteLine();
            Check00();
            LWMSCoreServer coreServer = new LWMSCoreServer();
            //coreServer.Bind("http://+:8080/");
            string p = Configuration.BasePath;
            coreServer.Start(100);
            Console.WriteLine("The server is now running good.");
            if (args.Length > 0)
            {
                var cmd = new List<CommandPack>();
                foreach (var item in _commands)
                {
                    cmd.Add(item);
                }
                ServerController.Control(cmd.ToArray());
            }
            CommandListener();
        }
        static void CommandListener()
        {
            PrintHint();
            while (true)
            {
                var cmd = Console.ReadLine();
                var cmdList = Tools00.ResolveCommand(cmd);
                ServerController.Control(cmdList.ToArray());
                PrintHint();
            }
        }
        static void PrintHint()
        {

            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Local");
            Console.ResetColor();
            Console.Write("]>");
        }
        static void Check00()
        {
            if (Configuration.ListenPrefixes.Count == 0)
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
                Configuration.ListenPrefixes = RecordedUrls;
                Console.WriteLine("Done!");
            }
        }
    }
}

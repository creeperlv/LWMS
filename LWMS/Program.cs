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
            if (args.Length > 0)
            {
                if (args[0] == "/PreBoot")
                {
                    var cmd = new List<string>(args);
                    cmd.RemoveAt(0);
                    ServerController.Control(cmd.ToArray());
                }
                else if (args[0] == "/NoBoot")
                {
                    var cmd = new List<string>(args);
                    cmd.RemoveAt(0);
                    ServerController.Control(cmd.ToArray());
                    return;
                }
                else { }
                
            }
            Console.WriteLine("LWMS - LightWeight Managed Server");
            Check00();
            LWMSCoreServer coreServer = new LWMSCoreServer();
            //coreServer.Bind("http://+:8080/");
            string p = Configuration.BasePath;
            coreServer.Start(100);
            Console.WriteLine("The server is now running good.");
            if (args.Length > 0)
            {
                ServerController.Control(args);
            }
            CommandListener();
        }
        static void CommandListener()
        {
            while (true)
            {
                var cmd=Console.ReadLine();
                var dcmd = Tools00.CommandParse(cmd);
                List<string> cmdList = new List<string>();
                foreach (var item in dcmd)
                {
                    cmdList.Add(item.Value);
                }
                ServerController.Control(cmdList.ToArray());
            }
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

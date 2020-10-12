using LWMS.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace LWMS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("LWMS - LightWeight Managed Server");
            Check00();
            LWMSCoreServer coreServer = new LWMSCoreServer();
            //coreServer.Bind("http://+:8080/");
            string p = Configuration.BasePath;
            coreServer.Start(100);
            Console.ReadLine();
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

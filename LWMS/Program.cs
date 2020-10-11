﻿using LWMS.Core;
using System;
using System.IO;

namespace LWMS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("LWMS - LightWeight Managed Server");
            LWMSCoreServer coreServer = new LWMSCoreServer();
            coreServer.Bind("http://+:8080/");
            coreServer.Start(100);
            string p = Configuration.BasePath;
            Console.ReadLine();
        }
    }
}

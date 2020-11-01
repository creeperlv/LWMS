﻿using LWMS.Core;
using LWMS.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LWMS.Management.Commands
{
    public class Runtimeconfig : IManageCommand
    {
        public string CommandName => "RuntimeConfig";
        List<string> alias = new List<string>();
        public Runtimeconfig()
        {
            alias.Add("rtcfg");
        }
        public List<string> Alias => alias;

        public int Version => 1;

        public void Invoke(params CommandPack[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var item = args[i];
                switch (item.ToUpper())
                {
                    case "/DISABLEBEAUTIFYCONSOLE":
                        LWMSTraceListener.BeautifyConsoleOutput = false;
                        break;
                    case "/ENABLEBEAUTIFYCONSOLE":
                        LWMSTraceListener.BeautifyConsoleOutput = true;
                        break;
                    case "/DISABLECONSOLE":
                        LWMSTraceListener.EnableConsoleOutput = false;
                        break;
                    case "/DISABLELOGTOFILE":
                        LWMSTraceListener.WriteToFile = false;
                        break;
                    case "/ENABLECONSOLE":
                        LWMSTraceListener.EnableConsoleOutput = true;
                        break;
                    default:
                        {
                            switch (item.PackParted[0].ToUpper())
                            {
                                case "BUF_LENGTH":
                                    {
                                        int B;
                                        int.TryParse(args[i].PackParted[1], out B);
                                        Configuration.Set_BUF_LENGTH_RT(B);
                                        Console.WriteLine($"BUT_LENGTH is set to {B} Byte(s), without saving to configuration file.");
                                    }
                                    break;
                                case "WEBROOT":
                                    {
                                        string path = args[i].PackParted[1];
                                        Configuration.Set_WebRoot_RT(path);
                                        Console.WriteLine($"Website content root is temporarily set to {path} without saving to configuration file.");
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                }
            }
        }
    }
}

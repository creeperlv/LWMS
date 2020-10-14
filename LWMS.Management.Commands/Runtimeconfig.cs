using LWMS.Core;
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
                    case "/DISABLECONSOLE":
                        LWMSTraceListener.EnableConsoleOutput = false;
                        break;
                    case "BUF_LENGTH":
                        {
                            Trace.Write($"You must specify the length with form like: -BUF_LENGTH=<Length>");
                        }
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
                                        Trace.WriteLine($"BUT_LENGTH is set to {B} Byte(s), without saving to configuration file.");
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

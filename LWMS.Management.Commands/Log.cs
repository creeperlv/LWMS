using LWMS.Core;
using LWMS.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LWMS.Management.Commands
{
    public class Log : IManageCommand
    {
        public string CommandName => "ManageLog";

        List<string> alias = new List<string>();
        public int Version => 2;
        public Log()
        {
            alias.Add("log");
        }
        public List<string> Alias => alias;

        public void Invoke(params CommandPack[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0].PackTotal.ToUpper())
                {
                    case "LS":
                    case "LIST":
                        {
                            foreach (var item in Directory.EnumerateFiles(LWMSTraceListener.LogDir))
                            {
                                Output.WriteLine(item);
                            }
                        }
                        break;
                    case "CLEAR":
                        {
                            foreach (var item in Directory.EnumerateFiles(LWMSTraceListener.LogDir))
                            {
                                try
                                {
                                    File.Delete(item);
                                    Output.WriteLine("Log>>Delete:" + item);
                                }
                                catch (Exception)
                                {
                                }
                            }

                        }
                        break;
                    case "NEW":
                        {
                            LWMSTraceListener.NewLogFile();
                        }
                        break;
                    case "HELP":
                    case "?":
                    case "-?":
                    case "--?":
                    case "--H":
                    case "-H":
                        {
                            Output.WriteLine("Log Unit");
                            Output.WriteLine("Usage:");
                            Output.WriteLine("LOG <OPERATION>");
                            Output.WriteLine("Operations:");
                            Output.WriteLine("\tList/LS");
                            Output.WriteLine("\t\tList all log files.");
                            Output.WriteLine("\tClear");
                            Output.WriteLine("\t\tDelete all old logs.(Except current using log file)");
                            Output.WriteLine("\tNEW");
                            Output.WriteLine("\t\tCreate a new log file and log contents to new log file.");
                            Output.WriteLine("\t\t*This operation is experimental.");
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

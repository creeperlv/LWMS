using LWMS.Core;
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
                                Trace.WriteLine(item);
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
                                    Trace.WriteLine("Log>>Delete:" + item);
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
                            Trace.WriteLine("Log Unit");
                            Trace.WriteLine("Usage:");
                            Trace.WriteLine("LOG <OPERATION>");
                            Trace.WriteLine("Operations:");
                            Trace.WriteLine("\tList/LS");
                            Trace.WriteLine("\t\tList all log files.");
                            Trace.WriteLine("\tClear");
                            Trace.WriteLine("\t\tDelete all old logs.(Except current using log file)");
                            Trace.WriteLine("\tNEW");
                            Trace.WriteLine("\t\tCreate a new log file and log contents to new log file.");
                            Trace.WriteLine("\t\t*This operation is experimental.");
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

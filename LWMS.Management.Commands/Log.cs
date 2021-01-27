using LWMS.Core;
using LWMS.Core.Log;
using LWMS.Localization;
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

        public void Invoke(string AuthContext, params CommandPack[] args)
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
                                    Output.WriteLine(Language.Query("ManageCmd.Log.Delete","Log>>Delete: {0}", item));
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
                    case "STOPWATCH":
                        {
                            LWMSTraceListener.StopWatch();
                        }
                        break;
                    case "HELP":
                    case "?":
                    case "-?":
                    case "--?":
                    case "--H":
                    case "-H":
                        {
                            Output.WriteLine(Language.Query("ManageCmd.Log.Name", "Log Manage Unit"));
                            Output.WriteLine("");
                            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Usage", "Usage:"));
                            Output.WriteLine("");
                            Output.WriteLine("LOG <OPERATION>");
                            Output.WriteLine("");
                            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Operations", "Operations:"));
                            Output.WriteLine("");
                            Output.WriteLine("\tList/LS");
                            Output.WriteLine(Language.Query("ManageCmd.Help.Log.List", "\t\tList all log files."));
                            Output.WriteLine("\tClear");
                            Output.WriteLine(Language.Query("ManageCmd.Help.Log.Clear", "\t\tDelete all old logs.(Except current using log file)"));
                            Output.WriteLine("\tNEW");
                            Output.WriteLine(Language.Query("ManageCmd.Help.Log.New", "\t\tCreate a new log file and log contents to the new log file."));
                            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Experimental", "\t\t*This operation is experimental."));
                            Output.WriteLine("\tStopwatch");
                            Output.WriteLine(Language.Query("ManageCmd.Help.Log.Stopwatch", "\t\tStop watching log cache, new logs will now be writtern to file."));
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

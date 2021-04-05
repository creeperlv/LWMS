using LWMS.Core;
using LWMS.Core.FileSystem;
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
        public int Version => 5;
        public Log()
        {
            alias.Add("log");
        }
        public List<string> Alias => alias;
        public void PrintHelp(string AuthContext)
        {

            {
                Output.WriteLine(Language.Query("ManageCmd.Log.Name", "Log Manage Unit"), AuthContext);
                Output.WriteLine("", AuthContext);
                Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Usage", "Usage:"), AuthContext);
                Output.WriteLine("", AuthContext);
                Output.WriteLine("LOG <OPERATION>", AuthContext);
                Output.WriteLine("", AuthContext);
                Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Operations", "Operations:"), AuthContext);
                Output.WriteLine("", AuthContext);
                Output.WriteLine("\tList/LS", AuthContext);
                Output.WriteLine(Language.Query("ManageCmd.Help.Log.List", "\t\tList all log files."), AuthContext);
                Output.WriteLine("\tClear", AuthContext);
                Output.WriteLine(Language.Query("ManageCmd.Help.Log.Clear", "\t\tDelete all old logs.(Except current using log file)"), AuthContext);
                Output.WriteLine("\tNEW", AuthContext);
                Output.WriteLine(Language.Query("ManageCmd.Help.Log.New", "\t\tCreate a new log file and log contents to the new log file."), AuthContext);
                Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Experimental", "\t\t*This operation is experimental."), AuthContext);
                Output.WriteLine("\tStopwatch", AuthContext);
                Output.WriteLine(Language.Query("ManageCmd.Help.Log.Stopwatch", "\t\tStop watching log cache, new logs will now be writtern to file."), AuthContext);
            }
        }
        public void Invoke(string AuthContext, params CommandPack[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0].PackTotal.ToUpper())
                {
                    case "LS":
                    case "LIST":
                        {
                            foreach (var item in ApplicationStorage.Logs.GetFiles(AuthContext))
                            {
                                Output.WriteLine(item.Name, AuthContext);
                            }
                        }
                        break;
                    case "CLEAR":
                        {
                            ApplicationStorage.Logs.DeleteAllItems(AuthContext, true);
                        }
                        break;
                    case "NEW":
                        {
                            LWMSTraceListener.NewLogFile(AuthContext);
                        }
                        break;
                    case "STOPWATCH":
                        {
                            LWMSTraceListener.StopWatch(AuthContext);
                        }
                        break;
                    case "HELP":
                    case "?":
                    case "-?":
                    case "--?":
                    case "--H":
                    case "-H":
                        PrintHelp(AuthContext);
                        break;
                    default:
                        break;
                }
            }
            else
                PrintHelp(AuthContext);
        }
    }
}

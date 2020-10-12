using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace LWMS.Core
{
    public class ServerController
    {
        public static void Control(params string[] args)
        {
            Trace.WriteLine("Received Command:" + args[0]);
            if (args[0].ToUpper() == "SHUTDOWN" || args[0].ToUpper() == "EXIT" || args[0].ToUpper() == "CLOSE")
            {
                Trace.WriteLine("Goodbye.");
                Environment.Exit(0);
            }
            else if (args[0].ToUpper() == "CONFIG")
            {
                if (args[1].ToUpper() == "RELEASE")
                {
                    Configuration.ConfigurationData.Dispose();
                    Configuration.ConfigurationData = null;
                    Trace.WriteLine("Configuration file is released and changes will not be saved.");
                }
                else if (args[1].ToUpper() == "RESUME")
                {
                    Configuration.LoadConfiguation();
                    Configuration.ClearLoadedSettings();
                    Trace.WriteLine("Resume");
                    Trace.WriteLine("Configuration changes will be automatically saved now.");
                }
            }
            else if (args[0].ToUpper() == "VER" || args[0].ToUpper() == "VERSION")
            {
                Trace.WriteLine("");
                Trace.WriteLine("Shell:" + Assembly.GetEntryAssembly());
                Trace.WriteLine("Core:" + Assembly.GetExecutingAssembly());
                Trace.WriteLine("");
            }
            else if (args[0].ToUpper() == "LOG")
            {
                if (args.Length > 1)
                {
                    switch (args[1].Trim().ToUpper())
                    {
                        case "LS":
                            {
                                foreach (var item in Directory.EnumerateFiles(LWMSTraceListener.LogDir))
                                {
                                    Trace.WriteLine(item);
                                }
                            }
                            break;
                        case "CLEAR":
                            {
                                try
                                {
                                    foreach (var item in Directory.EnumerateFiles(LWMSTraceListener.LogDir))
                                    {
                                        File.Delete(item);
                                        Trace.WriteLine("[Subprogram]Log>>Delete:" + item);
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (args[0].ToUpper() == "CLS" || args[0].ToUpper() == "CLEAR")
            {
                Console.Clear();
            }
            else if (args[0].ToUpper() == "RUNTIMECONFIG")
            {
                foreach (var item in args)
                {
                    if (item.ToUpper() == "/DISABLEBEAUTIFYCONSOLE")
                    {
                        LWMSTraceListener.BeautifyConsoleOutput = false;
                    }
                    if (item.ToUpper() == "/DISABLECONSOLE")
                    {
                        LWMSTraceListener.EnableConsoleOutput = false;
                    }
                }
            }
            else
            {
                Trace.WriteLine("Command Not Found.");
            }
        }
    }
}

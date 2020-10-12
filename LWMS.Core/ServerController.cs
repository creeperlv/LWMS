using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace LWMS.Core
{
    public class CommandPack
    {
        public string PackTotal;
        public List<string> PackParted = new List<string>();
        public static CommandPack FromRegexMatch(Match m)
        {
            CommandPack cp = new CommandPack();
            cp.PackTotal = m.Value.Trim();
            string _ = null;
            for (int i = 1; i < m.Groups.Count; i++)
            {
                if ((_ = m.Groups[i].Value.Trim()) != "")
                {
                    cp.PackParted.Add(_);
                }

            }
            return cp;
        }
        public static implicit operator string(CommandPack p)
        {
            return p.PackTotal;
        }
        public static implicit operator CommandPack(Match m)
        {
            return FromRegexMatch(m);
        }
        public string ToUpper()
        {
            return PackTotal.ToUpper();
        }
    }
    public class ServerController
    {
        public static void Control(params CommandPack[] args)
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
                    switch (args[1].PackTotal.ToUpper())
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
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].PackTotal.ToUpper() == "/DISABLEBEAUTIFYCONSOLE")
                    {
                        LWMSTraceListener.BeautifyConsoleOutput = false;
                    }
                    else
                    if (args[i].PackTotal.ToUpper() == "/DISABLECONSOLE")
                    {
                        LWMSTraceListener.EnableConsoleOutput = false;
                    }
                    else if (args[i].PackParted[0].ToUpper() == "BUF_LENGTH")
                    {
                        
                        int.TryParse(args[i].PackParted[1], out Configuration._BUF_LENGTH);
                        Trace.WriteLine($"BUT_LENGTH is set to {Configuration._BUF_LENGTH} Byte(s), without saving to configuration file.");
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

using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using LWMS.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace LWMS.Core
{
    public static class ServerController
    {
        public static Dictionary<string, IManageCommand> ManageCommands = new Dictionary<string, IManageCommand>();
        public static Dictionary<string, IManageCommand> ManageCommandAliases = new Dictionary<string, IManageCommand>();
        public static bool Register(string item)
        {
            try
            {
                var asm = Assembly.LoadFrom(item);
                var TPS = asm.GetTypes();
                foreach (var TP in TPS)
                {
                    if (typeof(IManageCommand).IsAssignableFrom(TP))
                    {
                        var MC = (IManageCommand)Activator.CreateInstance(TP);
                        Trace.WriteLine("Found Manage Command:" + MC.CommandName + "," + TP);
                        ManageCommands.Add(MC.CommandName, MC);
                        var alias = MC.Alias;
                        foreach (var MCA in alias)
                        {
                            ManageCommandAliases.Add(MCA, MC);
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                Trace.Write($"Cannot load management module:{item}");
                return false;
            }
        }

        public static void Control(params CommandPack[] args)
        {
            Trace.WriteLine("Received Command:" + args[0]);
            if (args[0].ToUpper() == "SHUTDOWN" || args[0].ToUpper() == "EXIT" || args[0].ToUpper() == "CLOSE")
            {
                Trace.WriteLine("Goodbye.");
                Environment.Exit(0);
            }
            else if (args[0].ToUpper() == "VER" || args[0].ToUpper() == "VERSION")
            {
                Trace.WriteLine("");
                Trace.WriteLine("Shell:" + Assembly.GetEntryAssembly());
                Trace.WriteLine("Core:" + Assembly.GetExecutingAssembly());
                Trace.WriteLine("");
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
                    else
                    if (args[i].PackTotal.ToUpper() == "/DISABLELOGTOFILE")
                    {
                        LWMSTraceListener.WriteToFile = false;
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
                foreach (var item in ManageCommands)
                {
                    if (item.Key.ToUpper() == args[0].PackTotal.ToUpper())
                    {
                        List<CommandPack> ManageCommandArgs = new List<CommandPack>(args);
                        try
                        {
                            ManageCommandArgs.RemoveAt(0);
                            try
                            {
                                item.Value.Invoke(ManageCommandArgs.ToArray());
                            }
                            catch (Exception e)
                            {
                                Trace.Write($"Error in {item.Value}: {e}");
                            }
                        }
                        catch (Exception)
                        {

                        }
                        return;
                    }
                }
                foreach (var item in ManageCommandAliases)
                {
                    if (item.Key.ToUpper() == args[0].PackTotal.ToUpper())
                    {
                        List<CommandPack> ManageCommandArgs = new List<CommandPack>(args);
                        try
                        {
                            ManageCommandArgs.RemoveAt(0);
                            try
                            {
                                item.Value.Invoke(ManageCommandArgs.ToArray());
                            }
                            catch (Exception e)
                            {
                                Trace.Write($"Error in {item.Value}: {e}");
                            }
                        }
                        catch (Exception)
                        {
                        }
                        return;
                    }
                }
                Trace.WriteLine("Command Not Found.");
            }
        }
    }
}
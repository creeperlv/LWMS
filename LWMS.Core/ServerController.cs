using LWMS.Core.Authentication;
using LWMS.Core.Configuration;
using LWMS.Core.Log;
using LWMS.Localization;
using LWMS.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace LWMS.Core
{
    public static class ServerController
    {
        public static Dictionary<string, IManageCommand> ManageCommands = new Dictionary<string, IManageCommand>();
        public static Dictionary<string, IManageCommand> ManageCommandAliases = new Dictionary<string, IManageCommand>();
        /// <summary>
        /// Register and initialize a specified module.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
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
                        Trace.WriteLine(Language.Query("LWMS.Commands.Found", "Found Manage Command:{0},{1}", MC.CommandName, TP.ToString()));
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
                Trace.Write(Language.Query("LWMS.Commands.Error.LoadModule", "Cannot load management module: {0}", item));
                return false;
            }
        }
        /// <summary>
        /// Response to the command packs.
        /// </summary>
        /// <param name="args"></param>
        public static void Control(string Auth, params CommandPack[] args)
        {
            Trace.WriteLine(Language.Query("LWMS.Commands.ReceieveCommand", "Received Command:", args[0]));
           
            if (OperatorAuthentication.IsAuthed(Auth, "ServerControl.ExecuteCommands"))
            {
                Trace.WriteLine(Language.Query("LWMS.Command.AuthReject", "Operation rejected: auth {0} have no permission.", Auth));
                return;
            }
            if (args[0].ToUpper() == "SHUTDOWN" || args[0].ToUpper() == "EXIT" || args[0].ToUpper() == "CLOSE")
            {
                Output.WriteLine("Goodbye.");
                if (LWMSTraceListener.WriteToFile)
                    LWMSTraceListener.FlushImmediately();
                Environment.Exit(0);
            }
            else if (args[0].ToUpper() == "VER" || args[0].ToUpper() == "VERSION")
            {
                Output.WriteLine("");
                Output.WriteLine(Language.Query("LWMS.Commands.Ver.Shell", "Shell: {0}", Assembly.GetEntryAssembly().ToString()));
                Output.WriteLine(Language.Query("LWMS.Commands.Ver.Core", "Core: {0}", Assembly.GetExecutingAssembly().ToString()));
                Output.WriteLine("");
            }
            else if (args[0].ToUpper() == "CLS" || args[0].ToUpper() == "CLEAR")
            {
                Console.Clear();
            }
            else if (args[0].ToUpper() == "SUSPEND")
            {
                //LWMSCoreServer.Listener.Stop();
                if (LWMSCoreServer.Listener != null)
                {

                    LWMSCoreServer.Listener.Abort();
                    LWMSCoreServer.Listener.Close();
                    LWMSCoreServer.Listener = null;
                    LWMSCoreServer.isSuspend = true;
                    Output.WriteLine(Language.Query("Server.Suspended", "Listener is now suspended."));
                }
            }
            else if (args[0].ToUpper() == "RESUME")
            {
                //LWMSCoreServer.Listener.Start();
                //                I do not know why HttpListener.Start() will not resume.
                if (LWMSCoreServer.Listener == null)
                {
                    LWMSCoreServer.Listener = new System.Net.HttpListener();

                    foreach (var item in GlobalConfiguration.ListenPrefixes)
                    {

                        LWMSCoreServer.Listener.Prefixes.Add(item);
                    }
                    LWMSCoreServer.Listener.Start();
                    LWMSCoreServer.isSuspend = false;
                    Output.WriteLine(Language.Query("Server.Resumed", "Listener is now resumed."));
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
                                Output.SetForegroundColor(ConsoleColor.Red);
                                Output.Write($"Error in {item.Value}: {e}");
                                Output.ResetColor();
                            }
                        }
                        catch (Exception)
                        {
                        }
                        return;
                    }
                }
                Output.WriteLine(Language.Query("LWMS.Commands.Error.NotFound", "Command Not Found."));
            }
        }
    }
}
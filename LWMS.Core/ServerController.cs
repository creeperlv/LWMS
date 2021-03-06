﻿using LWMS.Core.Authentication;
using LWMS.Core.Configuration;
using LWMS.Core.Log;
using LWMS.Core.SBSDomain;
using LWMS.Localization;
using LWMS.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace LWMS.Core
{
    public static class ServerController
    {
        public static Dictionary<string, MappedType> ManageCommands = new Dictionary<string, MappedType>();
        public static Dictionary<string, MappedType> ManageCommandAliases = new Dictionary<string, MappedType>();
        /// <summary>
        /// Register and initialize a specified module.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool Register(string AuthContext, string item)
        {
            bool __ = false;
            try
            {
                OperatorAuthentication.AuthedAction(AuthContext, () =>
                {
                    try
                    {
                        var asm = Assembly.LoadFrom(item);
                        FileInfo fi = new(item);
                        var TPS = asm.GetTypes();
                        foreach (var TP in TPS)
                        {
                            if (typeof(IManageCommand).IsAssignableFrom(TP))
                            {
                                var MC = (IManageCommand)Activator.CreateInstance(TP);
                                Trace.WriteLine(Language.Query("LWMS.Commands.Found", "Found Manage Command:{0},{1}", MC.CommandName, TP.ToString()));
                                ManageCommands.Add(MC.CommandName, MappedType.CreateFrom(MC));
                                var alias = MC.Alias;
                                foreach (var MCA in alias)
                                {
                                    ManageCommandAliases.Add(MCA, MappedType.CreateFrom(MC));
                                }
                            }
                        }
                        __ = true;
                    }
                    catch (Exception)
                    {
                        Trace.Write(Language.Query("LWMS.Commands.Error.LoadModule", "Cannot load management module: {0}", item));
                    }
                }, false, true, PermissionID.RegisterCmdModule, PermissionID.CmdModuleAll);

            }
            catch (Exception)
            {
                Trace.Write(Language.Query("LWMS.Commands.Error.LoadModule", "Cannot load management module: {0}", item));
            }
            return __;
        }
        /// <summary>
        /// Response to the command packs.
        /// </summary>
        /// <param name="args"></param>
        public static void Control(string Auth, params CommandPack[] args)
        {
            Trace.WriteLine(Language.Query("LWMS.Commands.ReceieveCommand", "Received Command:", args[0]));

            if (!OperatorAuthentication.IsAuthed(Auth, "Basic.ExecuteCommand"))
            {
                var name=OperatorAuthentication.GetAuthIDFromAuth(Auth);
                Trace.WriteLine(Language.Query("LWMS.Command.AuthReject", "Operation rejected: auth {0} have no permission.", name==null?Auth:name));
                return;
            }
            if (args[0].ToUpper() == "SHUTDOWN" || args[0].ToUpper() == "EXIT" || args[0].ToUpper() == "CLOSE")
            {
                try
                {

                    OperatorAuthentication.AuthedAction(Auth, () =>
                    {
                        Output.WriteLine(Language.Query("LWMS.Goodbye","Goodbye."),Auth);
                        if (LWMSTraceListener.WriteToFile)
                            LWMSTraceListener.FlushImmediately();
                        Environment.Exit(0);
                    }, false, false, "ServerControl.Shutdown", "ServerControl.All");
                }
                catch (Exception)
                {
                    Trace.WriteLine(Language.Query("LWMS.Auth.Reject", "Operation rejected: auth {0} have no permission of {1}.", Auth, "ServerControl.Shutdown"));
                }
            }
            else if (args[0].ToUpper() == "VER" || args[0].ToUpper() == "VERSION")
            {
                Output.WriteLine("", Auth);
                Output.WriteLine(Language.Query("LWMS.Commands.Ver.Shell", "Shell: {0}", Assembly.GetEntryAssembly().GetName().Version.ToString()), Auth);
                Output.WriteLine(Language.Query("LWMS.Commands.Ver.Core", "Core: {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString()), Auth);
                Output.WriteLine("", Auth);
            }
            else if (args[0].ToUpper() == "CLS" || args[0].ToUpper() == "CLEAR")
            {
                Output.Clear(Auth);
                //Console.Clear();
            }
            else if (args[0].ToUpper() == "SUSPEND")
            {

                try
                {

                    OperatorAuthentication.AuthedAction(Auth, () =>
                    {
                        if (LWMSCoreServer.Listener != null)
                        {

                            LWMSCoreServer.Listener.Abort();
                            LWMSCoreServer.Listener.Close();
                            LWMSCoreServer.Listener = null;
                            LWMSCoreServer.isSuspend = true;
                            Output.WriteLine(Language.Query("Server.Suspended", "Listener is now suspended."), Auth);
                        }
                    }, false, false, "ServerControl.Suspend", "ServerControl.ListenerControl", "ServerControl.All");
                }
                catch (Exception)
                {
                    Trace.WriteLine(Language.Query("LWMS.Auth.Reject", "Operation rejected: auth {0} have no permission of {1}.", Auth, "ServerControl.Suspend"));
                }
            }
            else if (args[0].ToUpper() == "RESUME")
            {
                //LWMSCoreServer.Listener.Start();
                //                I do not know why HttpListener.Start() will not resume.



                try
                {

                    OperatorAuthentication.AuthedAction(Auth, () =>
                    {
                        if (LWMSCoreServer.Listener == null)
                        {
                            LWMSCoreServer.Listener = new System.Net.HttpListener();

                            foreach (var item in GlobalConfiguration.GetListenPrefixes(LWMSCoreServer.TrustedInstallerAuth))
                            {

                                LWMSCoreServer.Listener.Prefixes.Add(item);
                            }
                            LWMSCoreServer.Listener.Start();
                            LWMSCoreServer.isSuspend = false;
                            Output.WriteLine(Language.Query("Server.Resumed", "Listener is now resumed."), Auth);
                        }
                    }, false, false, "ServerControl.Resume", "ServerControl.ListenerControl", "ServerControl.All");
                }
                catch (Exception)
                {
                    Trace.WriteLine(Language.Query("LWMS.Auth.Reject", "Operation rejected: auth {0} have no permission of {1}.", Auth, "ServerControl.Resume"));
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
                                (item.Value.TargetObject as IManageCommand).Invoke(Auth, ManageCommandArgs.ToArray());
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
                                (item.Value.TargetObject as IManageCommand).Invoke(Auth, ManageCommandArgs.ToArray());
                            }
                            catch (Exception e)
                            {
                                Output.SetForegroundColor(ConsoleColor.Red, Auth);
                                Output.Write($"Error in {item.Value}: {e}", Auth);
                                Output.ResetColor(Auth);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        return;
                    }
                }
                Output.WriteLine(Language.Query("LWMS.Commands.Error.NotFound", "Command Not Found."), Auth);
            }
        }
    }
}
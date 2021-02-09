using CLUNL.Pipeline;
using LWMS.Core.Authentication;
using LWMS.Core.Configuration;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Localization;
using LWMS.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LWMS.Core
{
    public class LWMSCoreServer
    {
        public static string ServerVersion = "Undefined";
        Semaphore semaphore;
        internal static HttpListener Listener;
        HttpPipelineProcessor HttpPipelineProcessor = new HttpPipelineProcessor();
        internal static string TrustedInstallerAuth;
        public LWMSCoreServer()
        {

            {
                var Auth0 = CLUNL.Utilities.RandomTool.GetRandomString(32, CLUNL.Utilities.RandomStringRange.R3);
                var Auth1 = CLUNL.Utilities.RandomTool.GetRandomString(32, CLUNL.Utilities.RandomStringRange.R3);
                TrustedInstallerAuth = OperatorAuthentication.ObtainRTAuth(Auth0, Auth1);
                OperatorAuthentication.SetTrustedInstaller(TrustedInstallerAuth);
                GlobalConfiguration.SetTrustedInstallerAuth(TrustedInstallerAuth);
            }
            Listener = new HttpListener();
            ServerVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString() + "-Preview";
        }
        bool WillStop = false;
        /// <summary>
        /// When isSuspend = true, listener will suspend waiting for new connection.
        /// </summary>
        public static bool isSuspend = false;
        List<IPipedProcessUnit> processUnits = new List<IPipedProcessUnit>();
        List<IPipedProcessUnit> CmdOutprocessUnits = new List<IPipedProcessUnit>();
        List<IPipedProcessUnit> WprocessUnits = new List<IPipedProcessUnit>();
        public void Start(int MaxThread)
        {
            semaphore = new Semaphore(MaxThread, MaxThread);
            Language.Initialize(GlobalConfiguration.Language);
            //Register Listener from beginning.
            RegisterProcessUnit(TrustedInstallerAuth, new LogUnit());
            //Add listening prefixes
            foreach (var item in GlobalConfiguration.GetListenPrefixes(TrustedInstallerAuth))
            {
                Listener.Prefixes.Add(item);
            }
            //Load process units.
            {
                foreach (var item in GlobalConfiguration.ListTSDRoot(TrustedInstallerAuth, 0))
                {
                    if (item.Value == "LWMS.Core.dll")
                    {
                        foreach (var UnitTypeName in GlobalConfiguration.ListTSDChild(TrustedInstallerAuth, 0, item.Key))
                        {
                            var t = Type.GetType(UnitTypeName.Value);
                            RegisterProcessUnit(TrustedInstallerAuth, (IPipedProcessUnit)Activator.CreateInstance(t));
                        }
                    }
                    else
                    {
                        try
                        {
                            FileInfo AssemblyFile = new FileInfo(item.Value);
                            var asm = Assembly.LoadFrom(AssemblyFile.FullName);
                            foreach (var UnitTypeName in GlobalConfiguration.ListTSDChild(TrustedInstallerAuth, 0, item.Key))
                            {
                                var t = asm.GetType(UnitTypeName.Value);
                                RegisterProcessUnit(TrustedInstallerAuth, Activator.CreateInstance(t) as IPipedProcessUnit);
                            }
                        }
                        catch (Exception)
                        {
                            Trace.WriteLine(Language.Query("LWMS.Pipeline.Error.Register.R", "Cannot load R pipeline units from: {0}", item.Value));
                        }
                    }
                }
            }
            RegisterProcessUnit(TrustedInstallerAuth, new ErrorResponseUnit());
            //Load W process units.
            {
                foreach (var item in GlobalConfiguration.ListTSDRoot(TrustedInstallerAuth,1))
                {
                    if (item.Value == "LWMS.Core.dll")
                    {
                        foreach (var UnitTypeName in GlobalConfiguration.ListTSDChild(TrustedInstallerAuth,1,item.Key))
                        {
                            var t = Type.GetType(UnitTypeName.Value);
                            RegisterWProcessUnit(TrustedInstallerAuth, (IPipedProcessUnit)Activator.CreateInstance(t));
                        }
                    }
                    else
                    {
                        try
                        {
                            FileInfo AssemblyFile = new FileInfo(item.Value);
                            var asm = Assembly.LoadFrom(AssemblyFile.FullName);
                            foreach (var UnitTypeName in GlobalConfiguration.ListTSDChild(TrustedInstallerAuth, 1, item.Key))
                            {
                                var t = asm.GetType(UnitTypeName.Value);
                                RegisterWProcessUnit(TrustedInstallerAuth, Activator.CreateInstance(t) as IPipedProcessUnit);
                            }
                        }
                        catch (Exception)
                        {
                            Trace.WriteLine(Language.Query("LWMS.Pipeline.Error.Register.W", "Cannot load W pipeline units from: {0}", item.Value));
                        }
                    }
                }
            }
            {
                {
                    foreach (var item in GlobalConfiguration.ListTSDRoot(TrustedInstallerAuth, 2))
                    {
                        if (item.Value == "LWMS.Management.dll")
                        {
                            var asm = Assembly.GetAssembly(typeof(Output));
                            foreach (var UnitTypeName in GlobalConfiguration.ListTSDChild(TrustedInstallerAuth, 2, item.Key))
                            {

                                var t = asm.GetType(UnitTypeName.Value);
                                RegisterCmdOutProcessUnit(TrustedInstallerAuth, (IPipedProcessUnit)Activator.CreateInstance(t));
                            }
                        }
                        else
                        {
                            try
                            {
                                FileInfo AssemblyFile = new FileInfo(item.Value);
                                var asm = Assembly.LoadFrom(AssemblyFile.FullName);
                                foreach (var UnitTypeName in GlobalConfiguration.ListTSDChild(TrustedInstallerAuth, 2, item.Key))
                                {
                                    var t = asm.GetType(UnitTypeName.Value);
                                    RegisterCmdOutProcessUnit(TrustedInstallerAuth, Activator.CreateInstance(t) as IPipedProcessUnit);
                                }
                            }
                            catch (Exception)
                            {
                                Trace.WriteLine(Language.Query("LWMS.Pipeline.Error.Register.CmdOut", "Cannot load CmdOut pipeline units from: {0}", item.Value));
                            }
                        }
                    }
                }
            }
            //Apply units
            ApplyProcessUnits(TrustedInstallerAuth);
            ApplyWProcessUnits(TrustedInstallerAuth);
            ApplyCmdProcessUnits(TrustedInstallerAuth);
            {
                //Catach all exceptions.
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }
            Listener.Start();
            Task.Run(() =>
            {
                while (WillStop == false)
                {
                    if (isSuspend == false)
                    {
                        semaphore.WaitOne();
                        //var __ = await Listener.GetContextAsync();
                        if (Listener != null)
                        {
                            if (Listener.IsListening)
                                try
                                {
                                    var a = Listener.BeginGetContext(new AsyncCallback((IAsyncResult r) =>
                                      {
                                          try
                                          {
                                              var __ = ((HttpListener)r.AsyncState).EndGetContext(r);
                                              ProcessContext_Internal(__);
                                              semaphore.Release(1);
                                          }
                                          catch (Exception)
                                          {
                                          }
                                      }), Listener);
                                    a.AsyncWaitHandle.WaitOne();
                                }
                                catch (Exception)
                                {
                                }
                        }
                        //if (isSuspend == true)
                        //{
                        //    __.Response.Close();
                        //    continue;
                        //}
                        //_ = Task.Run(() =>
                        //  {
                        //  });
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
            });
            //Load Manage Modules
            LoadCommandsFromManifest();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Output.SetForegroundColor(ConsoleColor.Red);
                Output.WriteLine("Unhandled Exception in:");
                Output.SetForegroundColor(ConsoleColor.Yellow);
                Output.WriteLine(e.ExceptionObject.ToString());
                if (e.IsTerminating)
                {
                    Output.SetForegroundColor(ConsoleColor.Red);
                    Output.WriteLine("LWMS will terminate.");
                }
                Output.ResetColor();
            }
            catch (Exception)
            {
                Console.WriteLine("Unhandled Exception in:");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("!Warning!Output pipeline is broken!");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(e.ExceptionObject.ToString());
                if (e.IsTerminating)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("LWMS will terminate.");
                }
                Console.ResetColor();
            }
        }

        public static void LoadCommandsFromManifest()
        {
            ServerController.ManageCommands.Clear();
            ServerController.ManageCommandAliases.Clear();
            foreach (string item in GlobalConfiguration.GetManageCommandModules(TrustedInstallerAuth))
            {
                ServerController.Register(TrustedInstallerAuth, item);
            }
        }

        public void RegisterProcessUnit(string Context, IPipedProcessUnit unit)
        {
            OperatorAuthentication.AuthedAction(Context, () =>
            {

                processUnits.Add(unit);
                Trace.WriteLine(Language.Query("LWMS.Pipeline.Register.R", "Registered R Unit: {0}", unit.GetType().ToString()));
            }, false, true, PermissionID.RTRegisterRProcessUnit, PermissionID.RuntimeAll);
        }
        public void UnregisterProcessUnit(string Context, IPipedProcessUnit unit)
        {

            OperatorAuthentication.AuthedAction(Context, () =>
            {

                processUnits.Remove(unit);
            }, false, true, PermissionID.RTUnregisterRProcessUnit, PermissionID.RuntimeAll);

        }

        public void RegisterWProcessUnit(string Context, IPipedProcessUnit unit)
        {
            OperatorAuthentication.AuthedAction(Context, () =>
            {
                WprocessUnits.Add(unit);
                Trace.WriteLine(Language.Query("LWMS.Pipeline.Register.W", "Registered W Unit: {0}", unit.GetType().ToString()));
            }, false, true, PermissionID.RTRegisterWProcessUnit, PermissionID.RuntimeAll);

        }
        public void UnregisterWProcessUnit(string Context, IPipedProcessUnit unit)
        {
            OperatorAuthentication.AuthedAction(Context, () =>
            {
                WprocessUnits.Remove(unit);
            }, false, true, PermissionID.RTUnregisterWProcessUnit, PermissionID.RuntimeAll);
        }
        public void RegisterCmdOutProcessUnit(string Context, IPipedProcessUnit unit)
        {
            OperatorAuthentication.AuthedAction(Context, () =>
            {
                CmdOutprocessUnits.Add(unit);
                Trace.WriteLine(Language.Query("LWMS.Pipeline.Register.CmdOut", "Registered CmdOut Unit: {0}", unit.GetType().ToString()));

            }, false, true, PermissionID.RTRegisterCmdOutProcessUnit, PermissionID.RuntimeAll);
        }
        public void UnregisterCmdOutProcessUnit(string Context, IPipedProcessUnit unit)
        {
            OperatorAuthentication.AuthedAction(Context, () =>
            {
                CmdOutprocessUnits.Remove(unit);
            }, false, true, PermissionID.RTUnregisterCmdOutProcessUnit, PermissionID.RuntimeAll);
        }
        public void ApplyProcessUnits(string Context)
        {
            OperatorAuthentication.AuthedAction(Context, () =>
            {
                HttpPipelineProcessor.Init(processUnits.ToArray());
            }, false, true, PermissionID.RTApplyRProcessUnits, PermissionID.RuntimeAll);
        }
        public void ApplyWProcessUnits(string Context)
        {
            OperatorAuthentication.AuthedAction(Context, () =>
            {
                PipelineStreamProcessor.DefaultPublicStreamProcessor.Init(WprocessUnits.ToArray());
            }, false, true, PermissionID.RTApplyWProcessUnits, PermissionID.RuntimeAll);
        }
        public void ApplyCmdProcessUnits(string Context)
        {

            OperatorAuthentication.AuthedAction(Context, () =>
            {
                var processor = new DefaultProcessor();
                processor.Init(CmdOutprocessUnits.ToArray());
                Output.SetCoreStream(Context, processor);
            }, false, true, PermissionID.RTApplyCmdProcessUnits, PermissionID.RuntimeAll);

        }
        internal void ProcessContext_Internal(HttpListenerContext context)
        {
            var a = new HttpListenerRoutedContext(context);
            var output = HttpPipelineProcessor.Process(new PipelineData(a, new HttpPipelineArguments(), null, context.GetHashCode()));
            (output.PrimaryData as HttpListenerRoutedContext).Response.OutputStream.Close();
        }
        public void ProcessContext(string AuthContext, HttpListenerContext context)
        {

        }
        public void Bind(string AuthContext, string URL)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () =>
            {
                Listener.Prefixes.Add(URL);
            }, false, true, PermissionID.BindPrefix);
        }
    }

}

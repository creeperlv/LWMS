﻿using CLUNL.Pipeline;
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
        public LWMSCoreServer()
        {
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
            RegisterProcessUnit(new LogUnit());
            //Add listening prefixes
            foreach (var item in GlobalConfiguration.ListenPrefixes)
            {
                Listener.Prefixes.Add(item);
            }
            //Load process units.
            {
                foreach (var item in GlobalConfiguration.RProcessUnits.RootNode.Children)
                {
                    if (item.Value == "LWMS.Core.dll")
                    {
                        foreach (var UnitTypeName in item.Children)
                        {
                            var t = Type.GetType(UnitTypeName.Value);
                            RegisterProcessUnit((IPipedProcessUnit)Activator.CreateInstance(t));
                        }
                    }
                    else
                    {
                        try
                        {
                            FileInfo AssemblyFile = new FileInfo(item.Value);
                            Assembly.LoadFrom(AssemblyFile.FullName);
                            foreach (var UnitTypeName in item.Children)
                            {
                                var t = Type.GetType(UnitTypeName.Value);
                                RegisterProcessUnit(Activator.CreateInstance(t) as IPipedProcessUnit);
                            }
                        }
                        catch (Exception)
                        {
                            Trace.WriteLine(Language.Query("LWMS.Pipeline.Error.Register.R", "Cannot load R pipeline units from: {0}", item.Value));
                        }
                    }
                }
            }
            RegisterProcessUnit(new ErrorResponseUnit());
            //Load W process units.
            {
                foreach (var item in GlobalConfiguration.WProcessUnits.RootNode.Children)
                {
                    if (item.Value == "LWMS.Core.dll")
                    {
                        foreach (var UnitTypeName in item.Children)
                        {
                            var t = Type.GetType(UnitTypeName.Value);
                            RegisterWProcessUnit((IPipedProcessUnit)Activator.CreateInstance(t));
                        }
                    }
                    else
                    {
                        try
                        {
                            FileInfo AssemblyFile = new FileInfo(item.Value);
                            Assembly.LoadFrom(AssemblyFile.FullName);
                            foreach (var UnitTypeName in item.Children)
                            {
                                var t = Type.GetType(UnitTypeName.Value);
                                RegisterWProcessUnit(Activator.CreateInstance(t) as IPipedProcessUnit);
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
                    foreach (var item in GlobalConfiguration.CMDOUTProcessUnits.RootNode.Children)
                    {
                        if (item.Value == "LWMS.Management.dll")
                        {
                            var asm = Assembly.GetAssembly(typeof(Output));
                            foreach (var UnitTypeName in item.Children)
                            {

                                var t = asm.GetType(UnitTypeName.Value);
                                RegisterCmdOutProcessUnit((IPipedProcessUnit)Activator.CreateInstance(t));
                            }
                        }
                        else
                        {
                            try
                            {
                                FileInfo AssemblyFile = new FileInfo(item.Value);
                                Assembly.LoadFrom(AssemblyFile.FullName);
                                foreach (var UnitTypeName in item.Children)
                                {
                                    var t = Type.GetType(UnitTypeName.Value);
                                    RegisterCmdOutProcessUnit(Activator.CreateInstance(t) as IPipedProcessUnit);
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
            ApplyProcessUnits();
            ApplyWProcessUnits();
            ApplyCmdProcessUnits();
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
                                              ProcessContext(__);
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
            foreach (string item in GlobalConfiguration.ManageCommandModules)
            {
                ServerController.Register(item);
            }
        }

        public void RegisterProcessUnit(IPipedProcessUnit unit)
        {
            processUnits.Add(unit);
            Trace.WriteLine(Language.Query("LWMS.Pipeline.Register.R", "Registered R Unit: {0}", unit.GetType().ToString()));
        }
        public void UnregisterProcessUnit(IPipedProcessUnit unit)
        {
            processUnits.Remove(unit);
        }

        public void RegisterWProcessUnit(IPipedProcessUnit unit)
        {
            WprocessUnits.Add(unit);
            Trace.WriteLine(Language.Query("LWMS.Pipeline.Register.W", "Registered W Unit: {0}", unit.GetType().ToString()));
        }
        public void UnregisterWProcessUnit(IPipedProcessUnit unit)
        {
            WprocessUnits.Remove(unit);
        }
        public void RegisterCmdOutProcessUnit(IPipedProcessUnit unit)
        {
            CmdOutprocessUnits.Add(unit);
            Trace.WriteLine(Language.Query("LWMS.Pipeline.Register.CmdOut", "Registered CmdOut Unit: {0}", unit.GetType().ToString()));
        }
        public void UnregisterCmdOutProcessUnit(IPipedProcessUnit unit)
        {
            CmdOutprocessUnits.Remove(unit);
        }
        public void ApplyProcessUnits()
        {
            HttpPipelineProcessor.Init(processUnits.ToArray());
        }
        public void ApplyWProcessUnits()
        {
            PipelineStreamProcessor.DefaultPublicStreamProcessor.Init(WprocessUnits.ToArray());
        }
        public void ApplyCmdProcessUnits()
        {
            var processor = new DefaultProcessor();
            processor.Init(CmdOutprocessUnits.ToArray());
            Output.CoreStream.Processor = processor;
        }
        public void ProcessContext(HttpListenerContext context)
        {
            var a = new HttpListenerRoutedContext(context);
            var output = HttpPipelineProcessor.Process(new PipelineData(a, new HttpPipelineArguments(), null, context.GetHashCode()));
            (output.PrimaryData as HttpListenerRoutedContext).Response.OutputStream.Close();
        }
        public void Bind(string URL)
        {
            Listener.Prefixes.Add(URL);
        }
    }

}

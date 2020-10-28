using CLUNL.Data.Layer0.Buffers;
using CLUNL.Pipeline;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LWMS.Core
{
    public class LWMSCoreServer
    {
        Semaphore semaphore;
        HttpListener Listener;
        HttpPipelineProcessor HttpPipelineProcessor = new HttpPipelineProcessor();
        public LWMSCoreServer()
        {
            Listener = new HttpListener();
        }
        bool WillStop = false;

        List<IPipedProcessUnit> processUnits = new List<IPipedProcessUnit>();
        List<IPipedProcessUnit> CmdOutprocessUnits = new List<IPipedProcessUnit>();
        List<IPipedProcessUnit> WprocessUnits = new List<IPipedProcessUnit>();
        public void Start(int MaxThread)
        {
            semaphore = new Semaphore(MaxThread, MaxThread);
            //Register Listener from beginning.
            RegisterProcessUnit(new LogUnit());
            //Add listening prefixes
            foreach (var item in Configuration.ListenPrefixes)
            {
                Listener.Prefixes.Add(item);
            }
            //Load process units.
            {
                foreach (var item in Configuration.RProcessUnits.RootNode.Children)
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
                            Trace.WriteLine("Cannot pipeline units from:" + item.Value);
                        }
                    }
                }
            }
            RegisterProcessUnit(new ErrorResponseUnit());
            //Load W process units.
            {
                foreach (var item in Configuration.WProcessUnits.RootNode.Children)
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
                            Trace.WriteLine("Cannot W pipeline units from:" + item.Value);
                        }
                    }
                }
            }
            {
                {
                    foreach (var item in Configuration.CMDOUTProcessUnits.RootNode.Children)
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
                                Trace.WriteLine("Cannot pipeline units from:" + item.Value);
                            }
                        }
                    }
                }
            }
            //Apply units
            ApplyProcessUnits();
            ApplyWProcessUnits();
            ApplyCmdProcessUnits(); 
            Listener.Start();
            Task.Run(async () =>
            {
                while (WillStop == false)
                {
                    semaphore.WaitOne();
                    var __ = await Listener.GetContextAsync();
                    _ = Task.Run(() =>
                      {
                          ProcessContext(__);
                          semaphore.Release(1);
                      });
                }
            });
            //Load Manage Modules
            LoadCommandsFromManifest();
        }
        public static void LoadCommandsFromManifest()
        {
            ServerController.ManageCommands.Clear();
            ServerController.ManageCommandAliases.Clear();
            foreach (string item in Configuration.ManageCommandModules)
            {
                ServerController.Register(item);
            }
        }

        public void RegisterProcessUnit(IPipedProcessUnit unit)
        {
            processUnits.Add(unit);
            Trace.WriteLine("Registered:" + unit.GetType());
        }
        public void UnregisterProcessUnit(IPipedProcessUnit unit)
        {
            processUnits.Remove(unit);
        }

        public void RegisterWProcessUnit(IPipedProcessUnit unit)
        {
            WprocessUnits.Add(unit);
            Trace.WriteLine("Registered W Unit:" + unit.GetType());
        }
        public void UnregisterWProcessUnit(IPipedProcessUnit unit)
        {
            WprocessUnits.Remove(unit);
        }
        public void RegisterCmdOutProcessUnit(IPipedProcessUnit unit)
        {
            CmdOutprocessUnits.Add(unit);
            Trace.WriteLine("Registered CmdOut Unit:" + unit.GetType());
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
            HttpPipelineProcessor.Process(new PipelineData(new HttpListenerRoutedContext(context), new HttpPipelineArguments(), null, context.GetHashCode()));
            context.Response.OutputStream.Close();
            //context.Response.OutputStream.Flush();
            //context.Response.OutputStream.Close();
            //var Response = context.Response;
            //Response.ContentType = "text/html";
            //var __ = Encoding.UTF8.GetBytes("<html><body><p>The server is row running.</p></body></html>");
            //Response.ContentLength64 = __.Length;
            //Response.StatusCode = 200;
            //Response.ContentEncoding = Encoding.UTF8;
            //Response.OutputStream.Write(__, 0, __.Length);
            //Response.OutputStream.Flush();
        }
        public void Bind(string URL)
        {
            Listener.Prefixes.Add(URL);
        }
    }

}

using CLUNL.Data.Layer0.Buffers;
using CLUNL.Pipeline;
using LWMS.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                foreach (var item in Configuration.ProcessUnits.RootNode.Children)
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
                        Assembly.LoadFrom(item.Value);
                        foreach (var UnitTypeName in item.Children)
                        {
                            var t = Type.GetType(UnitTypeName.Value);
                            RegisterProcessUnit(Activator.CreateInstance(t) as IPipedProcessUnit);
                        }
                    }
                }
            }
            RegisterProcessUnit(new ErrorResponseUnit());
            //RegisterProcessUnit(new DefaultStaticFileUnit());
            ApplyProcessUnits();
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
            foreach (string item in Configuration.ManageCommandModules)
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
                            Trace.WriteLine("Found Manage Command:" + MC.CommandName+","+TP);
                            ServerController.ManageCommands.Add(MC.CommandName, MC);
                        }
                    }
                }
                catch (Exception)
                {
                    Trace.Write($"Cannot load management module:{item}");
                }
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
        public void ApplyProcessUnits()
        {
            HttpPipelineProcessor.Init(processUnits.ToArray());
        }
        public void ProcessContext(HttpListenerContext context)
        {
            HttpPipelineProcessor.Process(new PipelineData(context, new HttpPipelineArguments(), null, context.GetHashCode()));
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

﻿using CLUNL.Data.Layer0.Buffers;
using CLUNL.Pipeline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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
            RegisterProcessUnit(new DefaultStaticFileUnit());
            ApplyProcessUnits();
            semaphore = new Semaphore(MaxThread, MaxThread);
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
        }
        public void RegisterProcessUnit(IPipedProcessUnit unit)
        {
            processUnits.Add(unit);
            Console.WriteLine("Registered:" + unit.GetType());
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
            HttpPipelineProcessor.Process(new PipelineData(context, null, null, context.GetHashCode()), false);
            Console.WriteLine("Completed.");
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

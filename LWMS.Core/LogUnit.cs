using CLUNL.DirectedIO;
using CLUNL.Pipeline;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LWMS.Core
{
    /// <summary>
    /// A pipeline unit will be automatically added into the first of the pipeline units definition to ensure picking up every request.
    /// </summary>
    public class LogUnit : IPipedProcessUnit
    {
        public LogUnit()
        {
            Trace.Listeners.Add(new LWMSTraceListener(Configuration.BasePath));
        }
        public PipelineData Process(PipelineData Input)
        {
            StringBuilder b = new StringBuilder();
            var c = Input.PrimaryData as HttpListenerRoutedContext;
            b.Append(c.Request.RemoteEndPoint);
            b.Append(">>");
            if (Configuration.LogUA)
            {
                b.Append(c.Request.UserAgent);
                b.Append(">>");
            }
            b.Append(c.Request.HttpMethod);
            b.Append(">>");
            b.Append(c.Request.RawUrl);
            Trace.WriteLine(b);
            return Input;
        }
    }
}

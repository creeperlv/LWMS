﻿using CLUNL.Pipeline;
using LWMS.Core.Configuration;
using LWMS.Core.FileSystem;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.Log;
using System.Diagnostics;
using System.Text;

namespace LWMS.Core
{
    /// <summary>
    /// A pipeline unit will be automatically added into the first of the pipeline units definition to ensure picking up every request.
    /// </summary>
    public class LogUnit : IPipedProcessUnit
    {
        public LogUnit()
        {
            Trace.Listeners.Add(new LWMSTraceListener(ApplicationStorage.BasePath));
        }
        public PipelineData Process(PipelineData Input)
        {
            var b = new StringBuilder();
            var c = Input.PrimaryData as HttpListenerRoutedContext;
            b.Append(c.Request.RemoteEndPoint);
            b.Append(">>");
            if (GlobalConfiguration.GetLogUA(LWMSCoreServer.TrustedInstallerAuth))
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

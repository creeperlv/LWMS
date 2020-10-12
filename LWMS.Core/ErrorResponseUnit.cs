using CLUNL.Pipeline;
using LWMS.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace LWMS.Core
{
    public class ErrorResponseUnit : IPipedProcessUnit
    {
        public PipelineData Process(PipelineData Input)
        {
            Trace.WriteLine("Unhandled Http Pipeline. R:"+(Input.PrimaryData as HttpListenerContext).Request.RawUrl);
            if (File.Exists(Configuration.Page404))
            {
                Tools00.SendFile(Input.PrimaryData as HttpListenerContext, new FileInfo(Configuration.Page404),404);
            }
            else
            {
                Tools00.SendMessage(Input.PrimaryData as HttpListenerContext,"<html><body><h1>404 File Not Found</h1><hr/><p>Hosted with LWMS.</p></body></html>",404);
                
            }
            (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
            return Input;
        }
    }
}

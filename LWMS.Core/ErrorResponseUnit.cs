using CLUNL.Pipeline;
using LWMS.Core.HttpRoutedLayer;
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
            //if (((HttpPipelineArguments)Input.SecondaryData).isHandled == true) return Input;
            HttpListenerRoutedContext context = Input.PrimaryData as HttpListenerRoutedContext;
            Trace.WriteLine("Unhandled Http Pipeline. R:"+(context).Request.RawUrl);
            if (File.Exists(Configuration.Page404))
            {
                Tools00.SendFile(context, new FileInfo(Configuration.Page404),404);
            }
            else
            {
                Tools00.SendMessage(context, "<html><body><h1>404 File Not Found</h1><hr/><p>Hosted with LWMS.</p></body></html>",404);
                
            }
            (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
            return Input;
        }
    }
}

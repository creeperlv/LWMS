using CLUNL.Pipeline;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.Utilities;
using LWMS.Localization;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace LWMS.Core
{
    /// <summary>
    /// This unit will response all requests that is not handled a 404 code and a 404 Page.
    /// </summary>
    public class ErrorResponseUnit : IPipedProcessUnit
    {
        public PipelineData Process(PipelineData Input)
        {
            //if (((HttpPipelineArguments)Input.SecondaryData).isHandled == true) return Input;
            HttpListenerRoutedContext context = Input.PrimaryData as HttpListenerRoutedContext;
            Trace.WriteLine(Language.Query("LWMS.ErrorResponseUnit.UnhandledRequest", "Unhandled Http Pipeline. Request: {0}", (context).Request.RawUrl));
            if (File.Exists(Configuration.Page404))
            {
                Tools00.SendFile(context, new FileInfo(Configuration.Page404), HttpStatusCode.NotFound);
            }
            else
            {
                Tools00.SendMessage(context, "<html><body><h1>404 File Not Found</h1><hr/><p>Hosted with LWMS.</p></body></html>", HttpStatusCode.NotFound);

            }
            (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
            return Input;
        }
    }
}

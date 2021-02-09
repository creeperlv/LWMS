using CLUNL.Pipeline;
using LWMS.Core.Configuration;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.Utilities;
using LWMS.Localization;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;

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
            Trace.WriteLine(Language.Query("LWMS.ErrorResponseUnit.UnhandledRequest", "Unhandled Http Pipeline. Request: {0}", HttpUtility.UrlDecode((context).Request.RawUrl)));


            if (context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                var _404 = GlobalConfiguration.GetPage404(LWMSCoreServer.TrustedInstallerAuth);
                if (File.Exists(_404))
                {
                    Tools00.SendFile(context, new FileInfo(_404), HttpStatusCode.NotFound);
                }
                else
                {
                    Tools00.SendMessage(context, "<html><body><h1>404 File Not Found</h1><hr/><p>Hosted with LWMS.</p></body></html>", HttpStatusCode.NotFound);

                }
            }
            else
            {
                Tools00.SendMessage(context, "<html><body><h1>Error 501</h1><hr/><p>Your request is not support yet by the server.</p></body></html>", HttpStatusCode.NotImplemented);
            }
            (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
            return Input;
        }
    }
}

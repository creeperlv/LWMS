using System;
using System.Collections.Generic;
using System.Text;
using CLUNL.Pipeline;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.Utilities;
using System.IO;

namespace LWMS.Core
{
    /// <summary>
    /// A basic unit that will perform as a static server: sending out static content on disk by calling Tools00.SendFile.
    /// </summary>
    public class DefaultStaticFileUnit : IPipedProcessUnit
    {
        public DefaultStaticFileUnit()
        {
        }
        public PipelineData Process(PipelineData Input)
        {
            HttpListenerRoutedContext context = Input.PrimaryData as HttpListenerRoutedContext;
            var path0 = context.Request.Url.LocalPath.Substring(1);
            var path1 = Path.Combine(Configuration.WebSiteContentRoot, path0);
            if (Directory.Exists(path1))
            {
                var DefaultPage = Path.Combine(path1, Configuration.DefaultPage);
                if (File.Exists(DefaultPage))
                {
                    Tools00.SendFile(context, new FileInfo(DefaultPage));
                    (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                }

            }
            else
            {

                if (File.Exists(path1))
                {
                    Tools00.SendFile(context, new FileInfo(path1));
                    (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                }
            }
            return Input;
        }

    }
}

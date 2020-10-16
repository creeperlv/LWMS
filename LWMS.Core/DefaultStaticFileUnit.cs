using System;
using System.Collections.Generic;
using System.Text;
using CLUNL.Pipeline;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.Utilities;
using System.IO;

namespace LWMS.Core
{
    public class DefaultStaticFileUnit : IPipedProcessUnit
    {
        public static int BUF_LENGTH = 1048576;
        public DefaultStaticFileUnit()
        {
        }
        public PipelineData Process(PipelineData Input)
        {
            HttpListenerRoutedContext context = Input.PrimaryData as HttpListenerRoutedContext;
            var path0 = context.Request.Url.LocalPath.Substring(1);
            var path1 = Path.Combine(Configuration.WebSiteContentRoot, path0);
            //Console.WriteLine("Try:"+path1);
            if (Directory.Exists(path1))
            {
                var DefaultPage = Path.Combine(path1, Configuration.DefaultPage);
                //Console.WriteLine("Try:" + DefaultPage);
                if (File.Exists(DefaultPage))
                {
                    Tools00.SendFile(context, new FileInfo(DefaultPage));
                    //return Input;
                    (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                }
                else
                {

                }

            }
            else
            {

                if (File.Exists(path1))
                {
                    Tools00.SendFile(context, new FileInfo(path1));
                    (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                }
                else
                {
                }
                //Console.WriteLine("Directory Not Found.");
            }
            return Input;
        }

    }
}

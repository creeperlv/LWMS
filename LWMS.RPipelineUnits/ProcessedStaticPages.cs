using CLUNL.Pipeline;
using LWMS.Core;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;

namespace LWMS.RPipelineUnits
{
    public class ProcessedStaticPages : IPipedProcessUnit
    {
        public static Dictionary<string, string> Prefixes = new Dictionary<string, string>();
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
                    if (path1.EndsWith("htm") || path1.EndsWith("html"))
                    {
                        ProcessContent(context, DefaultPage);

                        (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                    }
                }
                else
                {

                }

            }
            else
            {

                if (File.Exists(path1))
                {
                    if (path1.EndsWith("htm") || path1.EndsWith("html"))
                    {
                        ProcessContent(context, path1);
                        (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                    }
                }
                else
                {
                }
                //Console.WriteLine("Directory Not Found.");
            }
            return Input;
        }
        public static void ProcessContent(HttpListenerRoutedContext content, string path)
        {
            string Content = File.ReadAllText(path);
            {
                //Process.
                StringBuilder stringBuilder = new StringBuilder(Content);
                stringBuilder = stringBuilder.Replace("{OS.Description}", RuntimeInformation.OSDescription);
                stringBuilder = stringBuilder.Replace("{Runtime.Framework}", RuntimeInformation.FrameworkDescription);
                stringBuilder = stringBuilder.Replace("{LWMS.Core.Version}", Assembly.GetAssembly(typeof(LWMSCoreServer)).GetName().Version.ToString());
                stringBuilder = stringBuilder.Replace("{LWMS.Shell.Version}", Assembly.GetEntryAssembly().GetName().Version.ToString());
                stringBuilder = stringBuilder.Replace("{LWMS.Architect}", RuntimeInformation.ProcessArchitecture.ToString());
                stringBuilder = stringBuilder.Replace("{OS.Architect}", RuntimeInformation.OSArchitecture.ToString());
                stringBuilder = stringBuilder.Replace("{DateTime.Now}", DateTime.Now.ToString());
                foreach (var item in Prefixes)
                {
                    stringBuilder = stringBuilder.Replace($"{item.Key}", item.Value);
                }
                Tools00.SendMessage(content, stringBuilder.ToString());
            }
        }
    }
}

using CLUNL.Pipeline;
using LWMS.Core;
using LWMS.Core.FileSystem;
using LWMS.Core.HttpRoutedLayer;
using Markdig;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace LWMS.Sample.MarkdownBlog
{
    public class BlogMain : IPipedProcessUnit
    {
        public BlogMain()
        {
            Assembly.LoadFrom("Markdig.dll");
            //In case that dependency is not loaded.
        }
        StorageFile Template = null;
        StorageFolder Articles = null;
        public PipelineData Process(PipelineData Input)
        {
            if (Template is null)
                Template = ApplicationStorage.CurrentModule.GetFile("Template.html");
            if (Articles is null)
                Articles = ApplicationStorage.CurrentModule.CreateFolder("Articles",true);
            HttpListenerRoutedContext context = Input.PrimaryData as HttpListenerRoutedContext;
            var path0 = context.Request.Url.LocalPath.Substring(1);
            Console.WriteLine("MDBlog>>1");
            if (path0.StartsWith("blogs/"))
            {
                var path1 = path0.Substring(path0.IndexOf("/") + 1);
                var f = Articles.GetFile(path1);
                var MDContnet = Markdown.ToHtml(File.ReadAllText(f.ItemPath));
                var FinalContent = File.ReadAllText(Template.ItemPath).Replace("%Content%", MDContnet);
                context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(FinalContent));
                (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
            }
            return Input;
        }
    }
}

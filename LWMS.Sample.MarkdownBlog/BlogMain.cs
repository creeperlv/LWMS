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
        StorageFile ArticleTemplate = null;
        StorageFile ArticleListTemplate = null;
        StorageFile ArticleListItemTemplate = null;
        StorageFolder Articles = null;
        public PipelineData Process(PipelineData Input)
        {
            if (ArticleTemplate is null)
                ArticleTemplate = ApplicationStorage.CurrentModule.GetFile("Template.html");
            if (ArticleListTemplate is null)
                ArticleListTemplate = ApplicationStorage.CurrentModule.GetFile("ArticleListTemplate.html");
            if (ArticleListItemTemplate is null)
                ArticleListItemTemplate = ApplicationStorage.CurrentModule.GetFile("ArticleListItemTemplate.html");
            if (Articles is null)
                Articles = ApplicationStorage.CurrentModule.CreateFolder("Articles",true);
            HttpListenerRoutedContext context = Input.PrimaryData as HttpListenerRoutedContext;
            var path0 = context.Request.Url.LocalPath.Substring(1);
            Console.WriteLine("MDBlog>>1");
            if (path0.ToUpper().StartsWith("BLOGS/"))
            {
                var path1 = path0.Substring(path0.IndexOf("/") + 1);
                StorageFile f;
                
                if (Articles.GetFile(path1,out f,false))
                {
                    var MDContnet = Markdown.ToHtml(File.ReadAllText(f.ItemPath));
                    var FinalContent = File.ReadAllText(ArticleTemplate.ItemPath).Replace("%Content%", MDContnet);
                    context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(FinalContent));
                    (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                }
            }else if (path0.ToUpper() == "BLOGS")
            {

            }
            return Input;
        }
    }
}

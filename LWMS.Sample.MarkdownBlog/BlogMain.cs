using CLUNL.Pipeline;
using LWMS.Core;
using LWMS.Core.Configuration;
using LWMS.Core.FileSystem;
using LWMS.Core.HttpRoutedLayer;
using Markdig;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;

namespace LWMS.Sample.MarkdownBlog
{
    public class BlogMain : IPipedProcessUnit
    {
        public BlogMain()
        {
            SharedResources.Init();
        }
        public PipelineData Process(PipelineData Input)
        {
            {
                HttpListenerRoutedContext context = Input.PrimaryData as HttpListenerRoutedContext;
                var path0 = context.Request.Url.LocalPath.Substring(1);
                if (path0.ToUpper().StartsWith("BLOGS"))
                {
                    var path1 = path0.Substring(path0.IndexOf("/") + 1);
                    StorageFile f;
                    if (path1.ToUpper().EndsWith(".INFO"))
                    {

                    }
                    else
                    {
                        if (SharedResources.Articles.GetFile(path1, out f, false))
                        {
                            Trace.WriteLine("MDBlog>>Article:" + f.Name);
                            var MDContnet = File.ReadAllText(f.ItemPath);
                            if (SharedResources.isMarkdownUnavailable is false)
                            {
                                MDContnet = Markdown.ToHtml(MDContnet);
                                Trace.WriteLine("MDBlog>>Article>>MD");
                            }
                            string Title = f.Name;
                            StorageFile info;
                            if (SharedResources.Articles.GetFile(path1 + ".info", out info, false))
                            {
                                var infos = File.ReadAllLines(info.ItemPath);
                                if (infos.Length > 0)
                                    Title = infos[0];
                            }

                            var FinalContent = SharedResources.ArticleTemplate_.Replace("%Content%", MDContnet).Replace("%Title%", Title);
                            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(FinalContent));
                            (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                            return Input;
                        }
                    }
                    {
                        Trace.WriteLine("MDBlog>>MainPage");
                        var list = SharedResources.Articles.GetFiles();
                        var MainContent = SharedResources.ArticleListTemplate_;
                        var ItemTemplate = SharedResources.ArticleListItemTemplate_;
                        var ItemList = "";
                        string LinkPrefix = "./Blogs/";
                        if (path0.ToUpper().StartsWith("BLOGS/")) LinkPrefix = "./";
                        foreach (var item in list)
                        {
                            if (item.Name.ToUpper().EndsWith(".INFO"))
                            {
                                var infos = File.ReadAllLines(item.ItemPath);
                                if (infos.Length > 0)
                                    ItemList += ItemTemplate.Replace("%Title%", infos[0]).Replace("%Link%", LinkPrefix + item.Name.Substring(0, item.Name.Length - 5));
                            }
                        }
                        var FinalContent = MainContent.Replace("%Content%", ItemList).Replace("%Title%", ApplicationConfiguration.Current.GetValue("BlogTitle", "LWMS Blog"));
                        context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(FinalContent));
                        (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                        return Input;
                    }
                }
                (Input.SecondaryData as HttpPipelineArguments).isHandled = false;
                return Input;

            }
        }
    }
}

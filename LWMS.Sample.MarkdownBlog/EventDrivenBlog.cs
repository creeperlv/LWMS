using LWMS.Core.Configuration;
using LWMS.Core.FileSystem;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.Utilities;
using LWMS.EventDrivenSupport;
using Markdig;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LWMS.Sample.MarkdownBlog
{
    public class EventDrivenBlog : IHttpEventHandler
    {
        public EventDrivenBlog()
        {
            SharedResources.Init();
        }
        public bool Handle(HttpListenerRoutedContext context, string HttpPrefix)
        {
            {
                var path0 = context.Request.Url.LocalPath.Substring(1);
                if (path0.ToUpper().StartsWith(HttpPrefix.ToUpper()))
                {
                    var path1 = path0.Substring(HttpPrefix.Length);
                    //path1 = path1.Substring(path1.IndexOf("/") + 1);
                    StorageFile f;
                    if (path1.ToUpper().EndsWith(".INFO"))
                    {

                    }
                    else
                    {
                        if (SharedResources.Articles.GetFile(path1, out f, false))
                        {
                            Trace.WriteLine("MDBlog>>Article:" + f.Name);
                            if (!f.ItemPath.ToUpper().EndsWith(".MD"))
                            {
                                Tools00.SendFile(context, f);
                                return true;
                            }
                            var MDContnet = File.ReadAllText(f.ItemPath);
                            if (SharedResources.isMarkdownUnavailable is false)
                            {
                                MDContnet = Markdown.ToHtml(MDContnet);
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
                            return true;
                        }
                    }
                    {
                        Trace.WriteLine("MDBlog>>MainPage");
                        try
                        {

                            var list = SharedResources.Articles.GetFiles();

                            var MainContent = SharedResources.ArticleListTemplate_;
                            var ItemTemplate = SharedResources.ArticleListItemTemplate_;
                            var ItemList = "";
                            //string LinkPrefix = "./" + (HttpPrefix.Split("/").Last()) + "/";
                            
                            //if (path0.ToUpper().StartsWith(HttpPrefix.ToUpper())) 
                                var LinkPrefix = "./";
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
                            Tools00.SendMessage(context, FinalContent);
                            //context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(FinalContent));

                            return true;
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(e);
                        }
                    }
                }
                return false;

            }
        }
    }
}

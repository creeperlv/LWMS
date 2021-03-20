using LWMS.Core.Configuration;
using LWMS.Core.FileSystem;
using LWMS.Core.HttpRoutedLayer;
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
        bool isMarkdownUnavailable = false;
        public EventDrivenBlog()
        {

            try
            {
                Assembly.LoadFrom("Markdig.dll");
            }
            catch (Exception)
            {
                isMarkdownUnavailable = true;
                Trace.WriteLine("Cannot load Markdig.dll, MDBlog may be unable to work correctly.");
            }
            //In case that dependency is not loaded.
            BlogMain.CheckArticleTemplate();
            BlogMain.CheckHomepageItemTamplate();
            BlogMain.CheckHomepageTamplate();
            BlogMain.CheckArticleFolder();
        }
        public bool Handle(HttpListenerRoutedContext context,string HttpPrefix)
        {
            if (isMarkdownUnavailable is false)
            {
                BlogMain.CheckArticleTemplate();
                BlogMain.CheckHomepageItemTamplate();
                BlogMain.CheckHomepageTamplate();
                BlogMain.CheckArticleFolder();
                var path0 = context.Request.Url.LocalPath.Substring(1);
                if (path0.ToUpper().StartsWith(HttpPrefix.ToUpper()))
                {
                    var path1 = path0.Substring(HttpPrefix.Length).Substring(path0.IndexOf("/") + 1);
                    StorageFile f;
                    if (path1.ToUpper().EndsWith(".INFO"))
                    {

                    }
                    else
                    {
                        if (BlogMain.Articles.GetFile(path1, out f, false))
                        {
                            Trace.WriteLine("MDBlog>>Article:" + f.Name);
                            var MDContnet = Markdown.ToHtml(File.ReadAllText(f.ItemPath));
                            string Title = f.Name;
                            StorageFile info;
                            if (BlogMain.Articles.GetFile(path1 + ".info", out info, false))
                            {
                                var infos = File.ReadAllLines(info.ItemPath);
                                if (infos.Length > 0)
                                    Title = infos[0];
                            }

                            var FinalContent = File.ReadAllText(BlogMain.ArticleTemplate.ItemPath).Replace("%Content%", MDContnet).Replace("%Title%", Title);
                            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(FinalContent));
                            return true;
                        }
                    }
                    {
                        Trace.WriteLine("MDBlog>>MainPage");
                        var list = BlogMain.Articles.GetFiles();

                        var MainContent = File.ReadAllText(BlogMain.ArticleListTemplate.ItemPath);
                        var ItemTemplate = File.ReadAllText(BlogMain.ArticleListItemTemplate.ItemPath);
                        var ItemList = "";
                        string LinkPrefix = "./"+(HttpPrefix.Split("/").Last()) +"/";
                        if (!HttpPrefix.EndsWith("/"))
                        {
                            HttpPrefix += "/";
                        }
                        if (path0.ToUpper().StartsWith(HttpPrefix.ToUpper())) LinkPrefix = "./";
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

                        return true;
                    }
                }
                return false;

            }
            else
            {
                return false;
            }
        }
    }
}

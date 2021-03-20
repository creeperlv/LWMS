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
        bool isMarkdownUnavailable = false;
        public BlogMain()
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
            CheckArticleTemplate();
            CheckHomepageItemTamplate();
            CheckHomepageTamplate();
            CheckArticleFolder();
        }
        internal static StorageFile ArticleTemplate = null;
        internal static StorageFile ArticleListTemplate = null;
        internal static StorageFile ArticleListItemTemplate = null;
        internal static StorageFolder Articles = null;
        internal static void CheckArticleTemplate()
        {

            if (ArticleTemplate is null)
            {
                if (ApplicationStorage.CurrentModule.GetFile("Template.html", out ArticleTemplate) is false)
                {
                    ApplicationStorage.CurrentModule.CreateFile("Template.html", out ArticleTemplate);
                    File.WriteAllText(ArticleTemplate.ToFileInfo().FullName, DefaultPages.Template_html);
                }
            }
        }
        internal static void CheckHomepageTamplate()
        {
            if (ArticleListTemplate is null)
            {
                if (ApplicationStorage.CurrentModule.GetFile("ArticleListTemplate.html", out ArticleTemplate) is false)
                {
                    ApplicationStorage.CurrentModule.CreateFile("ArticleListTemplate.html", out ArticleTemplate);
                    File.WriteAllText(ArticleTemplate.ToFileInfo().FullName, DefaultPages.ArticleListTemplate_html);
                }
            }
        }
        internal static void CheckHomepageItemTamplate()
        {

            if (ArticleListItemTemplate is null)
            {
                if (ApplicationStorage.CurrentModule.GetFile("ArticleListItemTemplate.html", out ArticleTemplate) is false)
                {
                    ApplicationStorage.CurrentModule.CreateFile("ArticleListItemTemplate.html", out ArticleTemplate);
                    File.WriteAllText(ArticleTemplate.ToFileInfo().FullName, DefaultPages.ArticleListItemTemplate_html);
                }
            }
        }
        internal static void CheckArticleFolder()
        {

            if (Articles is null)
                Articles = ApplicationStorage.CurrentModule.CreateFolder("Articles", true);
        }
        public PipelineData Process(PipelineData Input)
        {
            if (isMarkdownUnavailable is false)
            {
                CheckArticleTemplate();
                CheckHomepageItemTamplate();
                CheckHomepageTamplate();
                CheckArticleFolder();
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
                        if (Articles.GetFile(path1, out f, false))
                        {
                            Trace.WriteLine("MDBlog>>Article:" + f.Name);
                            var MDContnet = Markdown.ToHtml(File.ReadAllText(f.ItemPath));
                            string Title = f.Name;
                            StorageFile info;
                            if (Articles.GetFile(path1 + ".info", out info, false))
                            {
                                var infos = File.ReadAllLines(info.ItemPath);
                                if (infos.Length > 0)
                                    Title = infos[0];
                            }

                            var FinalContent = File.ReadAllText(ArticleTemplate.ItemPath).Replace("%Content%", MDContnet).Replace("%Title%", Title);
                            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(FinalContent));
                            (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                            return Input;
                        }
                    }
                    {
                        Trace.WriteLine("MDBlog>>MainPage");
                        var list = Articles.GetFiles();

                        var MainContent = File.ReadAllText(ArticleListTemplate.ItemPath);
                        var ItemTemplate = File.ReadAllText(ArticleListItemTemplate.ItemPath);
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
            else
            {
                (Input.SecondaryData as HttpPipelineArguments).isHandled = false;
                return Input;
            }
        }
    }
}

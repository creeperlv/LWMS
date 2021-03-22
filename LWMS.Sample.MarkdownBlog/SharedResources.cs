using LWMS.Core.FileSystem;
using LWMS.Core.ScheduledTask;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LWMS.Sample.MarkdownBlog
{
    public class SharedResources : IScheduleTask
    {
        internal static StorageFile ArticleTemplate = null;
        internal static StorageFile ArticleListTemplate = null;
        internal static StorageFile ArticleListItemTemplate = null;
        internal static StorageFolder Articles = null;
        internal static bool isInited = false;
        internal static string ArticleTemplate_ = "";
        internal static string ArticleListTemplate_ = "";
        internal static string ArticleListItemTemplate_ = "";
        internal static void Init()
        {
            if (isInited is false)
            {
                isInited = true;
                CheckArticleTemplate();
                CheckHomepageItemTamplate();
                CheckHomepageTamplate();
                CheckArticleFolder();
                ScheduleTask.Schedule(typeof(SharedResources), "MarkdownBlog.FixedTimeUpdate", ScheduleTaskGap.Sec10);
            }
        }

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
                if (ApplicationStorage.CurrentModule.GetFile("ArticleListTemplate.html", out ArticleListTemplate) is false)
                {
                    ApplicationStorage.CurrentModule.CreateFile("ArticleListTemplate.html", out ArticleListTemplate);
                    File.WriteAllText(ArticleListTemplate.ToFileInfo().FullName, DefaultPages.ArticleListTemplate_html);
                }
            }
        }
        internal static void CheckHomepageItemTamplate()
        {

            if (ArticleListItemTemplate is null)
            {
                if (ApplicationStorage.CurrentModule.GetFile("ArticleListItemTemplate.html", out ArticleListItemTemplate) is false)
                {
                    ApplicationStorage.CurrentModule.CreateFile("ArticleListItemTemplate.html", out ArticleListItemTemplate);
                    File.WriteAllText(ArticleListItemTemplate.ToFileInfo().FullName, DefaultPages.ArticleListItemTemplate_html);
                }
            }
        }
        internal static void CheckArticleFolder()
        {

            if (Articles is null)
                Articles = ApplicationStorage.CurrentModule.CreateFolder("Articles", true);
        }
        public void Task()
        {
            try
            {

                CheckArticleTemplate();
                CheckHomepageItemTamplate();
                CheckHomepageTamplate();
                CheckArticleFolder();
            }
            catch (Exception)
            {

            }
            try
            {

                ArticleListItemTemplate_ = File.ReadAllText(ArticleListTemplate.ItemPath);
                ArticleTemplate_ = File.ReadAllText(ArticleTemplate.ItemPath);
                ArticleListItemTemplate_ = File.ReadAllText(ArticleListItemTemplate.ItemPath);

            }
            catch (Exception)
            {
            }
        }
    }
}

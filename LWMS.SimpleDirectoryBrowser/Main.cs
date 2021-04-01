using System;
using System.IO;
using LWMS.Core.Configuration;
using LWMS.Core.FileSystem;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.ScheduledTask;
using LWMS.Core.Utilities;
using LWMS.EventDrivenSupport;

namespace LWMS.SimpleDirectoryBrowser
{
    public class Main : IHttpEventHandler
    {
        public Main()
        {
            ScheduleTask.Schedule(typeof(UpdateTemplateTask), "SimpleDirectoryBrowser.UpdateTask", ScheduleTaskGap.Sec30);
        }
        public bool Handle(HttpListenerRoutedContext context, string HttpPrefix)
        {
            var MappedDirectory = ApplicationConfiguration.Current.GetValue(HttpPrefix, null);
            if (MappedDirectory == null)
            {
                MappedDirectory = ApplicationConfiguration.Current.GetValue("*");
                if (MappedDirectory == null) MappedDirectory = GlobalConfiguration.GetWebSiteContentRoot(context.PipelineAuth);
            }
            var path = Path.Combine(MappedDirectory, context.Request.Url.LocalPath.Substring(1).Substring(HttpPrefix.Length));
            if (Directory.Exists(path))
            {
                //Visit Directory
            }
            else
            {
                //Visit File
                if (File.Exists(path))
                {
                    FileInfo fi = new FileInfo(path);
                    if (Tools00.ObtainMimeType(fi.Extension).StartsWith("text"){

                    }
                    else
                    {
                        Tools00.SendFile(context, fi);
                    }
                }
            }
            return false;
        }
        public static string ToHTMLString(string content)
        {
            return content.Replace(" ", "&nbsp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\t", "&#09;").Replace("\"", "&quot;").Replace("&", "&amp;").Replace("\r", "&#13;").Replace("\n", "&#10;");
        }
    }
    internal static class SharedResources
    {
        static SharedResources()
        {
            Update();
        }
        internal static string DirectoryTemplate;
        internal static string DirectoryItemFolderTemplate;
        internal static string DirectoryItemFileTemplate;
        internal static string TextViewerTemplate;
        internal static StorageFile DirectoryTemplateFile;
        internal static StorageFile DirectoryItemFolderTemplateFile;
        internal static StorageFile DirectoryItemFileTemplateFile;
        internal static StorageFile TextViewerTemplateFile;

        internal static void Update()
        {
            bool ExtractedDefaultTemplate = false;
            if (DirectoryTemplateFile is null)
            {
                if (ApplicationStorage.CurrentModule.CreateFile("DirectoryTemplateFile.html", out DirectoryTemplateFile) is true)
                {
                    DirectoryTemplateFile.WriteAllText(DefaultTemplates.DirectoryTemplate);
                    ExtractedDefaultTemplate = true;
                }
            }
            if (DirectoryItemFileTemplateFile is null)
            {
                if (ApplicationStorage.CurrentModule.CreateFile("DirectoryItemFileTemplateFile.html", out DirectoryItemFileTemplateFile) is true)
                {
                    DirectoryItemFileTemplateFile.WriteAllText(DefaultTemplates.DirectoryItemFileTemplate);
                    ExtractedDefaultTemplate = true;
                }
            }
            if (DirectoryItemFolderTemplateFile is null)
            {
                if (ApplicationStorage.CurrentModule.CreateFile("DirectoryItemFolderTemplateFile.html", out DirectoryItemFolderTemplateFile) is true)
                {
                    DirectoryItemFolderTemplateFile.WriteAllText(DefaultTemplates.DirectoryItemFolderTemplate);
                    ExtractedDefaultTemplate = true;
                }
            }
            if (TextViewerTemplateFile is null)
            {
                if (ApplicationStorage.CurrentModule.CreateFile("TextViewerTemplateFile.html", out TextViewerTemplateFile) is true)
                {
                    TextViewerTemplateFile.WriteAllText(DefaultTemplates.TextViewerTemplate);
                    ExtractedDefaultTemplate = true;
                }
            }

            if (ExtractedDefaultTemplate is true)
            {
                Update();
                return;
            }
            {
                //Real Load

                TextViewerTemplate = File.ReadAllText(TextViewerTemplateFile.ItemPath);
                DirectoryItemFolderTemplate = File.ReadAllText(DirectoryItemFolderTemplateFile.ItemPath);
                DirectoryItemFileTemplate = File.ReadAllText(DirectoryItemFileTemplateFile.ItemPath);
                DirectoryTemplate = File.ReadAllText(DirectoryTemplateFile.ItemPath);
            }
        }
    }
    public class UpdateTemplateTask : IScheduleTask
    {

        void IScheduleTask.Task()
        {
            SharedResources.Update();
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
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
            string CutPath = context.Request.Url.LocalPath.Substring(1).Substring(HttpPrefix.Length);
            var path = Path.Combine(MappedDirectory, CutPath);
            Trace.WriteLine($"Final:{CutPath}");
            if (Directory.Exists(path))
            {
                //Visit Directory
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                StringBuilder folders = new StringBuilder();
                StringBuilder files = new StringBuilder();
                {
                    folders.Append(SharedResources.DirectoryItemFolderTemplate.Replace("%Name%", "..").Replace("%Date%", "")).Replace("%Link%", "./../");
                    foreach (var item in directoryInfo.EnumerateDirectories())
                    {
                        folders.Append(SharedResources.DirectoryItemFolderTemplate.Replace("%Name%", item.Name).Replace("%Date%", item.LastWriteTimeUtc + "")).Replace("%Link%", "./" + item.Name + "/");
                    }
                    foreach (var item in directoryInfo.EnumerateFiles())
                    {
                        files.Append(SharedResources.DirectoryItemFileTemplate.Replace("%Name%", item.Name).Replace("%Date%", item.LastWriteTimeUtc + "").Replace("%Size%", Math.Round(((double)item.Length) / 1024.0) + " KB")).Replace("%Link%", "./" + item.Name);
                    }

                }
                var content = SharedResources.DirectoryTemplate.Replace("%Folders%", folders.ToString()).Replace("%Files%", files.ToString()).Replace("%Address%", ToHTMLString(context.Request.Url.LocalPath));
                Tools00.SendMessage(context, content);
                return true;
            }
            else
            {
                //Visit File
                if (File.Exists(path))
                {
                    FileInfo fi = new FileInfo(path);
                    if (Tools00.ObtainMimeType(fi.Extension).StartsWith("text"))
                    {
                        var content = SharedResources.TextViewerTemplate.Replace("%Content%",ToHTMLString( File.ReadAllText(fi.FullName))).Replace("%Address%", ToHTMLString(context.Request.Url.LocalPath));
                        Tools00.SendMessage(context, content);
                        return true;
                    }
                    else
                    {
                        Tools00.SendFile(context, fi);
                        return true;
                    }
                }
            }
            return false;
        }
        public static string ToHTMLString(string content)
        {
            return content.Replace("&", "&amp;").Replace(" ", "&nbsp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\t", "&#09;").Replace("\"", "&quot;").Replace("\n", "<br/>");
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
        internal static StorageFile DirectoryTemplateFile=null;
        internal static StorageFile DirectoryItemFolderTemplateFile = null;
        internal static StorageFile DirectoryItemFileTemplateFile = null;
        internal static StorageFile TextViewerTemplateFile = null;

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

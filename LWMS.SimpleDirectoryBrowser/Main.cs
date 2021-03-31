using System;
using System.IO;
using LWMS.Core.Configuration;
using LWMS.Core.FileSystem;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.ScheduledTask;
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
            }
            return false;
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
            if (DirectoryItemFileTemplateFile is null)
            {
                if(ApplicationStorage.CurrentModule.GetFile("DirectoryTemplateFile.html", out DirectoryTemplateFile) is false)
                {
                    ExtractedDefaultTemplate = true;
                }
            }
            if(ExtractedDefaultTemplate is true)
            {
                Update();
                return;
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

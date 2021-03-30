using System;
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


            return false;
        }
    }
    internal static class SharedResources
    {
        internal static string DirectoryTemplate;
        internal static string TextViewerTemplate;
        internal static StorageFile DirectoryTemplateFile;
        internal static StorageFile TextViewerTemplateFile;
    }
    public class UpdateTemplateTask : IScheduleTask
    {

        void IScheduleTask.Task()
        {
        }
    }
}

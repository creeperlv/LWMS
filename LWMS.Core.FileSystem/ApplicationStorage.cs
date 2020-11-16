using LWMS.Core.Configuration;
using System;

namespace LWMS.Core.FileSystem
{
    public static class ApplicationStorage
    {
        static ApplicationStorage()
        {
            Webroot = new StorageFolder();
            Webroot.Parent = null;
            Webroot.realPath = GlobalConfiguration.WebSiteContentRoot;
        }
        public static StorageFolder Webroot { get; internal set; }
    }
}

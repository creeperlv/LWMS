using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using LWMS.Core.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace LWMS.Core.FileSystem
{
    public static class ApplicationStorage
    {
        static INILikeData RouteLocationMap;
        static Dictionary<string, string> Map=new Dictionary<string, string>();
        static ApplicationStorage()
        {
            RouteLocationMap = INILikeData.LoadFromWR(new FileWR(new FileInfo(Path.Combine(GlobalConfiguration.BasePath,"RoutedLocations.ini"))));
            foreach (var item in RouteLocationMap)
            {
                Map.Add(item.Key, item.Value);
            }
            {
                Webroot = new StorageFolder();
                Webroot.Parent = SystemRoot;
                Webroot.realPath = GlobalConfiguration.WebSiteContentRoot;
            }
            {
                SystemRoot = new StorageFolder();
                SystemRoot.Parent = null;
                SystemRoot.realPath = "{Root}";
            }
        }
        public static StorageItem ObtainItemFromRelativeURL(string URL)
        {
            StorageItem storageItem = new StorageItem();
            string[] paths;
            if (URL.IndexOf('/') > 0)
                paths = URL.Split('/');
            else paths = URL.Split('\\');
            for (int i = 0; i < paths.Length; i++)
            {

            }
            return null;
        }

        public static StorageFolder SystemRoot { get; internal set; }
        public static StorageFolder Webroot { get; internal set; }
    }
}

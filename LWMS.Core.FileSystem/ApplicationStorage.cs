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
        static Dictionary<string, string> Map = new Dictionary<string, string>();
        static ApplicationStorage()
        {
            RouteLocationMap = INILikeData.LoadFromWR(new FileWR(new FileInfo(Path.Combine(GlobalConfiguration.BasePath, "RoutedLocations.ini"))));
            foreach (var item in RouteLocationMap)
            {
                Map.Add(item.Key.Replace('\\', '/'), item.Value);
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
        /// <summary>
        /// Obtain an item from a relative url.
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="CaseSensitivity"></param>
        /// <returns></returns>
        public static StorageItem ObtainItemFromRelativeURL(string URL,bool CaseSensitivity=false)
        {
            StorageItem storageItem = new StorageItem();
            string[] paths;
            URL = URL.Replace('\\', '/');
            StorageFolder Root = Webroot;
            foreach (var item in Map)
            {
                if (URL.StartsWith(item.Key))
                {
                    Root = new StorageFolder();
                    Root.parent = SystemRoot;
                    storageItem.SetPath(item.Value);
                    URL = URL.Substring(item.Key.Length);
                    break;
                }
            }

            if (URL.IndexOf('/') > 0)
                paths = URL.Split('/');
            else paths = URL.Split('\\');
            for (int i = 0; i < paths.Length; i++)
            {
                if (i + 1 == paths.Length)
                {
                    //Get Final Item.
                    return Root.GetContainedItem(paths[i], CaseSensitivity);
                }
                else
                {
                    Root = Root.GetContainedFolder(paths[i], CaseSensitivity);
                }
            }
            return null;
        }

        public static StorageFolder SystemRoot { get; internal set; }
        public static StorageFolder Webroot { get; internal set; }
    }
}

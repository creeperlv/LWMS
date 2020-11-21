using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using LWMS.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace LWMS.Core.FileSystem
{
    public static class ApplicationStorage
    {
        static INILikeData RouteLocationMap;
        static Dictionary<string, string> Map = new Dictionary<string, string>();
        static ApplicationStorage()
        {
            try
            {
                if (!File.Exists(Path.Combine(GlobalConfiguration.BasePath, "RoutedLocations.ini")))
                {
                    RouteLocationMap = INILikeData.CreateToFile(new FileInfo(Path.Combine(GlobalConfiguration.BasePath, "RoutedLocations.ini")));
                }
                else
                    RouteLocationMap = INILikeData.LoadFromWR(new FileWR(new FileInfo(Path.Combine(GlobalConfiguration.BasePath, "RoutedLocations.ini"))));
                foreach (var item in RouteLocationMap)
                {
                    Map.Add(item.Key.Replace('\\', '/'), item.Value);
                }
            }
            catch (Exception)
            {
            }
            {
                SystemRoot = new StorageFolder();
                SystemRoot.parent = null;
                SystemRoot.realPath = "{Root}";
            }
            {
                Webroot = new StorageFolder();
                Webroot.Parent = SystemRoot;
                Webroot.isroot = true;
                Webroot.realPath = GlobalConfiguration.WebSiteContentRoot;
            }
        }
        /// <summary>
        /// Remove a route definition
        /// </summary>
        /// <param name="Key"></param>
        public static void RemoveRoute(string Key)
        {
            Map.Remove(Key);
            RouteLocationMap.DeleteKey(Key, true);
        }
        /// <summary>
        /// Add a route definition
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public static void AddRoute(string Key, string Value)
        {
            Map.Add(Key, Value);
            RouteLocationMap.AddValue(Key, Value, false, true);
        }
        /// <summary>
        /// Obtain an item from a relative url.
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="CaseSensitivity"></param>
        /// <returns></returns>
        public static StorageItem ObtainItemFromRelativeURL(string URL, bool CaseSensitivity = false)
        {
            Trace.WriteLine("Receieved:" + URL);
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
                    Root.isroot = true;
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
        public static bool ObtainItemFromRelativeURL(string URL, out StorageItem OutItem, bool CaseSensitivity = false)
        {
            StorageItem storageItem = new StorageItem();
            URL=HttpUtility.UrlDecode(URL);
            string[] paths;
            URL = URL.Replace('\\', '/');
            StorageFolder Root = Webroot;
            foreach (var item in Map)
            {
                if (URL.StartsWith(item.Key))
                {
                    Root = new StorageFolder();
                    Root.parent = SystemRoot;
                    Root.isroot = true;
                    storageItem.SetPath(item.Value);
                    URL = URL.Substring(item.Key.Length);
                    break;
                }
            }
            if (URL == "")
            {
                OutItem = Root;
                return true;
            }
            if (URL.IndexOf('/') > 0)
                paths = URL.Split('/');
            else paths = URL.Split('\\');
            for (int i = 0; i < paths.Length; i++)
            {
                if (i + 1 == paths.Length)
                {
                    //Get Final Item.
                    return Root.GetContainedItem(paths[i], out OutItem, CaseSensitivity);
                }
                else
                {
                    if (Root.GetContainedFolder(paths[i], out Root, CaseSensitivity) == false)
                    {
                        OutItem = null;
                        return false;
                    }
                }
            }
            OutItem = null;
            return false;
        }
        /// <summary>
        /// A virtual folder that contains nothing, indicates that a folder whose parent is SystemRoot is a root folder.
        /// </summary>
        public static StorageFolder SystemRoot { get; internal set; }
        /// <summary>
        /// Webroot, same folder as GlobalConfiguration.WebSiteContentRoot.
        /// </summary>
        public static StorageFolder Webroot { get; internal set; }
    }
}

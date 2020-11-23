using CLUNL;
using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;

namespace LWMS.Core.FileSystem
{
    public static class ApplicationStorage
    {
        static INILikeData RouteLocationMap;
        static Dictionary<string, string> Map = new Dictionary<string, string>();
        static string BasePath;
        static ApplicationStorage()
        {
                BasePath = new FileInfo(Assembly.GetAssembly(typeof(ApplicationStorage)).Location).DirectoryName;
            {
                SystemRoot = new StorageFolder();
                SystemRoot.parent = null;
                SystemRoot.realPath = "{Root}";
            }
            {
                _Webroot = new StorageFolder();
                _Webroot.Parent = SystemRoot;
                _Webroot.isroot = true;
            }
            {
                Configuration = new StorageFolder();
                Configuration.Parent = SystemRoot;
                Configuration.isroot = true;
                Configuration.realPath = Path.Combine(BasePath, "Configurations");
                if (!Directory.Exists(Configuration.realPath))
                {
                    Directory.CreateDirectory(Configuration.realPath);
                }
            }
            {
                Logs = new StorageFolder();
                Logs.Parent = SystemRoot;
                Logs.isroot = true;
                Logs.realPath = Path.Combine(BasePath, "Logs");
                if (!Directory.Exists(Logs.realPath))
                {
                    Directory.CreateDirectory(Logs.realPath);
                }
            }
            try
            {
                StorageFile Routes;
                _=Configuration.CreateFile("RoutedLocations.ini", out Routes);
                RouteLocationMap = INILikeData.LoadFromWR(new FileWR(Routes));
                foreach (var item in RouteLocationMap)
                {
                    Map.Add(item.Key.Replace('\\', '/'), item.Value);
                }
            }
            catch (Exception)
            {
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
                    return Root.GetItem(paths[i], CaseSensitivity);
                }
                else
                {
                    Root = Root.GetFolder(paths[i], CaseSensitivity);
                }
            }
            return null;
        }
        /// <summary>
        /// Obtain an item from a relative url, return false if target item cannot be found.
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="OutItem"></param>
        /// <param name="CaseSensitivity"></param>
        /// <returns></returns>
        public static bool ObtainItemFromRelativeURL(string URL, out StorageItem OutItem, bool CaseSensitivity = false)
        {
            StorageItem storageItem = new StorageItem();
            URL = HttpUtility.UrlDecode(URL);
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
                    return Root.GetItem(paths[i], out OutItem, CaseSensitivity);
                }
                else
                {
                    if (Root.GetFolder(paths[i], out Root, CaseSensitivity) == false)
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
        static StorageFolder _Webroot;
        public static void SetRealWebRoot(string WebRootPath)
        {
            _Webroot.SetPath(WebRootPath);
        }
        /// <summary>
        /// Webroot, same folder as GlobalConfiguration.WebSiteContentRoot.
        /// </summary>
        public static StorageFolder Webroot
        {
            get
            {
                return _Webroot;
            }
            internal set { _Webroot = value; }
        }
        public static StorageFolder Configuration { get; internal set; }
        public static StorageFolder Logs { get; internal set; }
    }
}

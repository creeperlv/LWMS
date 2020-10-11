using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace LWMS.Core
{
    public static class Configuration
    {
        static Configuration()
        {
            BasePath = new FileInfo(Assembly.GetAssembly(typeof(LWMSCoreServer)).Location).DirectoryName;
            var ConfigurationPath = Path.Combine(BasePath, "Server.ini");
            if (File.Exists(ConfigurationPath))
            {
                ConfigurationData = INILikeData.LoadFromStream((new FileInfo(ConfigurationPath)).Open(FileMode.Open));
            }
            else
            {
                ConfigurationData = INILikeData.CreateToFile(new FileInfo(ConfigurationPath));
            }
        }
        public static INILikeData ConfigurationData;
        public readonly static string BasePath;
        static string _WebSiteContentRoot = null;
        static string _DefultPage = null;
        public static string WebSiteContentRoot
        {
            get
            {
                if (_WebSiteContentRoot == null)
                {
                    try
                    {
                        _WebSiteContentRoot = ConfigurationData.FindValue("WebContentRoot");
                        if (_WebSiteContentRoot == null)
                        {
                            _WebSiteContentRoot = Path.Combine(BasePath, "webroot");
                            ConfigurationData.AddValue("WebContentRoot", _WebSiteContentRoot, AutoSave : true) ;
                        }
                    }
                    catch
                    {
                        _WebSiteContentRoot = Path.Combine(BasePath, "webroot");
                        ConfigurationData.AddValue("WebContentRoot", _WebSiteContentRoot, AutoSave: true);
                    }
                }
                return _WebSiteContentRoot;
            }
            set { _WebSiteContentRoot = value;
                ConfigurationData.AddValue("WebContentRoot", _WebSiteContentRoot, AutoSave: true); }
        }
        public static string DefaultPage
        {
            get
            {
                if (_DefultPage == null)
                {
                    try
                    {
                        _DefultPage = ConfigurationData.FindValue("DefaultPage");
                        if (_DefultPage == null)
                        {
                            _DefultPage = "index.html"; ConfigurationData.AddValue("DefaultPage", _DefultPage, AutoSave: true);
                        }
                    }
                    catch { _DefultPage = "index.html"; ConfigurationData.AddValue("DefaultPage", _DefultPage, AutoSave: true); }
                }
                return _DefultPage;
            }
            set { _DefultPage = value; ConfigurationData.AddValue("DefaultPage", _DefultPage, AutoSave: true); }
        }
    }
}

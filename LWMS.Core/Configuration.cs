using CLUNL.Data.Layer0;
using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace LWMS.Core
{
    public static class Configuration
    {
        static Configuration()
        {
            BasePath = new FileInfo(Assembly.GetAssembly(typeof(LWMSCoreServer)).Location).DirectoryName;
            var ConfigurationPath = Path.Combine(BasePath, "Server.ini");
            var PluginConfigPath = Path.Combine(BasePath, "Plugin.tsd");
            if (File.Exists(ConfigurationPath))
            {
                ConfigurationData = INILikeData.LoadFromStream((new FileInfo(ConfigurationPath)).Open(FileMode.Open));
            }
            else
            {
                ConfigurationData = INILikeData.CreateToFile(new FileInfo(ConfigurationPath));
            }
            if (File.Exists(PluginConfigPath))
            {
                ProcessUnits = TreeStructureData.LoadFromFile(new FileInfo(PluginConfigPath));
            }
            else
            {
                ProcessUnits = TreeStructureData.CreateToFile(new FileInfo(PluginConfigPath));
                {
                    //LWMS.Core.dll
                    TreeNode treeNode = new TreeNode();
                    treeNode.Name = "DLL";
                    treeNode.Value = "LWMS.Core.dll";
                    {
                        TreeNode unit = new TreeNode();
                        unit.Name = "Unit.0";
                        unit.Value = "LWMS.Core.DefaultStaticFileUnit";
                        treeNode.AddChildren(unit);
                    }
                ProcessUnits.RootNode.AddChildren(treeNode);
                }
                ProcessUnits.Serialize();
            }
        }
        public static TreeStructureData ProcessUnits;
        public static INILikeData ConfigurationData;
        public readonly static string BasePath;
        static string _WebSiteContentRoot = null;
        static string _DefultPage = null;
        static string _Page404 = null;
        static List<string> _ListenPrefixes = new List<string>();
        public static string Page404
        {
            get
            {
                if (_Page404 == null)
                {
                    try
                    {
                        _Page404 = ConfigurationData.FindValue("Page_404");
                        if (_WebSiteContentRoot == null)
                        {
                            _Page404 = Path.Combine(WebSiteContentRoot, "Page_404");
                            ConfigurationData.AddValue("Page_404", _Page404, AutoSave: true);
                        }
                    }
                    catch
                    {
                        _Page404 = Path.Combine(WebSiteContentRoot, "Page_404");
                        ConfigurationData.AddValue("Page_404", _Page404, AutoSave: true);
                    }
                }
                return _Page404;
            }
            set
            {
                _Page404 = value;
                        ConfigurationData.AddValue("Page_404", _Page404, AutoSave: true);
            }
        }
        public static List<string> ListenPrefixes
        {
            get
            {
                if (_ListenPrefixes.Count == 0)
                {
                    var cs = ConfigurationData.FindValue("Prefix.Count");
                    if (cs == null)
                    {
                        return _ListenPrefixes;
                    }
                    else
                    {
                        int c = int.Parse(cs);
                        for (int i = 0; i < c; i++)
                        {
                            _ListenPrefixes.Add(ConfigurationData.FindValue($"Prefix.{i}"));
                        }
                    }
                }
                return _ListenPrefixes;
            }
            set
            {
                _ListenPrefixes = value;
                ConfigurationData.AddValue("Prefix.Count", _ListenPrefixes.Count + "", true, false);
                for (int i = 0; i < ConfigurationData.Count; i++)
                {
                    ConfigurationData.AddValue($"Prefix.{i}", _ListenPrefixes[i], true, false);
                }
                ConfigurationData.RemoveOldDuplicatedItems();
                ConfigurationData.Flush();
            }
        }
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
                            ConfigurationData.AddValue("WebContentRoot", _WebSiteContentRoot, AutoSave: true);
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
            set
            {
                _WebSiteContentRoot = value;
                ConfigurationData.AddValue("WebContentRoot", _WebSiteContentRoot, AutoSave: true);
            }
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

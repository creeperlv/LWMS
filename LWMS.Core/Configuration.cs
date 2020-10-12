using CLUNL.Data.Layer0;
using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        static int _BUF_LENGTH = 0;
        static List<string> _ListenPrefixes = new List<string>();
        public static int BUF_LENGTH
        {
            get
            {
                if (_BUF_LENGTH == 0)
                {

                    var cs = ConfigurationData.FindValue("BUF_LENGTH");
                    if (cs == null)
                    {
                        _BUF_LENGTH = 1024 * 128;//128KB per buf.
                        ConfigurationData.AddValue("BUF_LENGTH", _BUF_LENGTH + "", AutoSave: true);
                    }
                    else
                    {
                        try
                        {
                            _BUF_LENGTH = int.Parse(cs);
                        }
                        catch (Exception)
                        {
                            _BUF_LENGTH = 1024 * 128;//128KB per buf.
                            ConfigurationData.AddValue("BUF_LENGTH", _BUF_LENGTH + "", AutoSave: true);
                        }
                    }

                }
                return _BUF_LENGTH;
            }
            set
            {
                _BUF_LENGTH = value;
                ConfigurationData.AddValue("BUF_LENGTH", _BUF_LENGTH + "", AutoSave: true);
            }
        }
        public static string Page404
        {
            get
            {
                if (_Page404 == null)
                {
                    try
                    {
                        _Page404 = ConfigurationData.FindValue("Page_404");
                        if (_Page404 == null)
                        {
                            Trace.WriteLine("Generating default 404 page path.");
                            _Page404 = Path.Combine(WebSiteContentRoot, "Page_404.html");
                            ConfigurationData.AddValue("Page_404", _Page404, AutoSave: true);
                        }
                    }
                    catch
                    {
                        Trace.WriteLine("Generating default 404 page path.");
                        _Page404 = Path.Combine(WebSiteContentRoot, "Page_404.html");
                        ConfigurationData.AddValue("Page_404", _Page404, AutoSave: true);
                    }
                    ConfigurationData.Flush();
                }
                return _Page404;
            }
            set
            {
                _Page404 = value;
                ConfigurationData.AddValue("Page_404", _Page404, AutoSave: true);
                ConfigurationData.Flush();
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
                for (int i = 0; i < _ListenPrefixes.Count; i++)
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
                    ConfigurationData.Flush();

                }
                return _WebSiteContentRoot;
            }
            set
            {
                _WebSiteContentRoot = value;
                ConfigurationData.AddValue("WebContentRoot", _WebSiteContentRoot, AutoSave: true);
                ConfigurationData.Flush();
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
                    ConfigurationData.Flush();
                }
                return _DefultPage;
            }
            set
            {
                _DefultPage = value; ConfigurationData.AddValue("DefaultPage", _DefultPage, AutoSave: true);
                ConfigurationData.Flush();
            }
        }
    }
}

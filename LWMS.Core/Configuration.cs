﻿using CLUNL;
using CLUNL.Data.Layer0;
using CLUNL.Data.Layer1;
using CLUNL.Data.Layer2;
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
        static string ConfigurationPath = null;
        static Configuration()
        {
            LibraryInfo.SetFlag(FeatureFlags.Pipeline_AutoID_Random, 1);
            BasePath = new FileInfo(Assembly.GetAssembly(typeof(LWMSCoreServer)).Location).DirectoryName;
            ConfigurationPath = Path.Combine(BasePath, "Server.ini");
            var PluginConfigPath = Path.Combine(BasePath, "RPipelineUnit.tsd");
            var WPipelineUnitsPath = Path.Combine(BasePath, "WPipelineUnit.tsd");
            var ManageModulePath = Path.Combine(BasePath, "ManageModules.ini");
            LoadConfiguation();
            if (File.Exists(ManageModulePath))
            {
                ManageCommandModules = ListData<string>.LoadFromStream(File.Open(ManageModulePath, FileMode.Open, FileAccess.ReadWrite));
            }
            else
            {
                ManageCommandModules = ListData<string>.CreateToFile(new FileInfo(ManageModulePath));
                ManageCommandModules.Add(Path.Combine(BasePath, "LWMS.Management.Commands.dll"));
                ManageCommandModules.Save();
            }
            {
                //Load Request Pipeline Units.
                if (File.Exists(PluginConfigPath))
                {
                    RProcessUnits = TreeStructureData.LoadFromFile(new FileInfo(PluginConfigPath));
                }
                else
                {
                    RProcessUnits = TreeStructureData.CreateToFile(new FileInfo(PluginConfigPath));
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
                        RProcessUnits.RootNode.AddChildren(treeNode);
                    }
                    RProcessUnits.Serialize();
                }
            }
            {
                //Load W Pipelne units.
                if (File.Exists(WPipelineUnitsPath))
                {
                    WProcessUnits = TreeStructureData.LoadFromFile(new FileInfo(WPipelineUnitsPath));
                }
                else
                {
                    WProcessUnits = TreeStructureData.CreateToFile(new FileInfo(WPipelineUnitsPath));
                    {
                        //LWMS.Core.dll
                        TreeNode treeNode = new TreeNode();
                        treeNode.Name = "DLL";
                        treeNode.Value = "LWMS.Core.dll";
                        {
                            TreeNode unit = new TreeNode();
                            unit.Name = "DefaultUnit";
                            unit.Value = "LWMS.Core.HttpRoutedLayer.DefaultStreamProcessUnit";
                            treeNode.AddChildren(unit);
                        }
                        WProcessUnits.RootNode.AddChildren(treeNode);
                    }
                    WProcessUnits.Serialize();
                }
            }
        }
        public static void LoadConfiguation()
        {
            if (File.Exists(ConfigurationPath))
            {
                ConfigurationData = INILikeData.LoadFromStream((new FileInfo(ConfigurationPath)).Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
            }
            else
            {
                ConfigurationData = INILikeData.CreateToFile(new FileInfo(ConfigurationPath));
            }

        }

        public static TreeStructureData RProcessUnits;
        public static TreeStructureData WProcessUnits;
        public static INILikeData ConfigurationData;
        public static ListData<string> ManageCommandModules;

        public readonly static string BasePath;
        internal static string _WebSiteContentRoot = null;
        internal static string _DefultPage = null;
        internal static string _Page404 = null;
        internal static bool? _LogUA = null;
        internal static int _BUF_LENGTH = 0;
        public static void Set_BUF_LENGTH_RT(int VALUE)
        {
            _BUF_LENGTH = VALUE;
        }
        public static void Set_WebRoot_RT(string Path)
        {
            _WebSiteContentRoot = Path;
        }
        internal static List<string> _ListenPrefixes = new List<string>();
        public static void ClearLoadedSettings()
        {
            _WebSiteContentRoot = null;
            _DefultPage = null;
            _Page404 = null;
            _BUF_LENGTH = 0;
            _ListenPrefixes = new List<string>();
        }
        public static bool LogUA
        {
            get
            {
                if (_LogUA == null)
                {
                    var value = ConfigurationData.FindValue("LogUA");
                    if (value == null)
                    {
                        _LogUA = false;
                        ConfigurationData.AddValue("LogUA", _LogUA + "", true, true);
                    }
                    else
                    {
                        _LogUA = bool.Parse(value);
                    }
                }

                return _LogUA == true ? true : false;
            }
            set
            {
                _LogUA = value;
                ConfigurationData.AddValue("LogUA", _LogUA + "", true, true);

            }
        }
        public static int BUF_LENGTH
        {
            get
            {
                if (_BUF_LENGTH == 0)
                {
                    try
                    {

                        var cs = ConfigurationData.FindValue("BUF_LENGTH");
                        if (cs == null)
                        {
                            _BUF_LENGTH = 1048576;//1MB per buf.
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
                                _BUF_LENGTH = 1048576;//1MB per buf.
                                ConfigurationData.AddValue("BUF_LENGTH", _BUF_LENGTH + "", AutoSave: true);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Trace.WriteLine("Cannot save configuration.");

                    }

                }
                return _BUF_LENGTH;
            }
            set
            {
                _BUF_LENGTH = value;
                if (ConfigurationData != null)
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
                        Trace.WriteLine("Cannot save configuration.");
                    }
                    ConfigurationData.Flush();
                }
                return _Page404;
            }
            set
            {
                _Page404 = value;
                if (ConfigurationData != null)
                    ConfigurationData.AddValue("Page_404", _Page404, AutoSave: true);
                if (ConfigurationData != null)
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

                if (ConfigurationData != null)
                {

                    ConfigurationData.AddValue("Prefix.Count", _ListenPrefixes.Count + "", true, false);
                    for (int i = 0; i < _ListenPrefixes.Count; i++)
                    {
                        ConfigurationData.AddValue($"Prefix.{i}", _ListenPrefixes[i], true, false);
                    }
                    ConfigurationData.RemoveOldDuplicatedItems();
                    ConfigurationData.Flush();
                }
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
                        Trace.WriteLine("Cannot save configuration.");

                    }
                    ConfigurationData.Flush();

                }
                return _WebSiteContentRoot;
            }
            set
            {
                _WebSiteContentRoot = value;
                if (ConfigurationData != null)
                    ConfigurationData.AddValue("WebContentRoot", _WebSiteContentRoot, AutoSave: true);
                if (ConfigurationData != null)
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
                    catch
                    {
                        Trace.WriteLine("Cannot save configuration.");

                    }
                    ConfigurationData.Flush();
                }
                return _DefultPage;
            }
            set
            {
                _DefultPage = value;
                if (ConfigurationData != null)
                {
                    ConfigurationData.AddValue("DefaultPage", _DefultPage, AutoSave: true);
                    ConfigurationData.Flush();
                }
            }
        }
    }
}

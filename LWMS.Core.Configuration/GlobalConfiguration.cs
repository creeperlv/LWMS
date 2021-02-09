using CLUNL;
using CLUNL.Data.Layer0;
using CLUNL.Data.Layer1;
using CLUNL.Data.Layer2;
using CLUNL.Utilities;
using LWMS.Core.Authentication;
using LWMS.Core.FileSystem;
using LWMS.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace LWMS.Core.Configuration
{
    public class GlobalConfiguration
    {
        internal static string TrustedInstaller = null;
        public static void SetTrustedInstallerAuth(string auth)
        {
            if (TrustedInstaller == null)
            {
                TrustedInstaller = auth;
                {
                    try
                    {
                        _WebSiteContentRoot = null;
                        _ = WebSiteContentRoot;
                        ApplicationStorage.SetRealWebRoot(TrustedInstaller, _WebSiteContentRoot);
                    }
                    catch
                    {
                    }
                    try
                    {
                        _WebModuleStorage = null;
                        _ = WebSiteModuleStorageRoot;
                        ApplicationStorage.SetRealModuleRoot(TrustedInstaller, _WebModuleStorage);
                    }
                    catch { }
                }
            }
        }
        static GlobalConfiguration()
        {
            LibraryInfo.SetFlag(FeatureFlags.Pipeline_AutoID_Random, 1);
            LibraryInfo.SetFlag(FeatureFlags.FileWR_AutoCreateFile, 1);
            LoadConfiguation();
            {
                StorageFile ManageModuleFile;
                if (ApplicationStorage.Configuration.CreateFile("ManageModules.ini", out ManageModuleFile))
                {
                    Trace.WriteLine(Localization.Language.Query("LWMS.Config.CreateDefaultCommandManifest", "Create default command module manifest."));
                    ManageCommandModules = ListData<string>.LoadFromStream(ManageModuleFile.OpenFile());
                    ManageCommandModules.Add(Path.Combine(ApplicationStorage.BasePath, "LWMS.Management.Commands.dll"));
                    ManageCommandModules.Save();
                }
                else
                {
                    ManageCommandModules = ListData<string>.LoadFromStream(ManageModuleFile.OpenFile());
                }
            }
            {
                StorageFile RPipelineUnitDefinitionFile;
                if (ApplicationStorage.Configuration.CreateFile("RPipelineUnit.tsd", out RPipelineUnitDefinitionFile))
                {
                    RProcessUnits = TreeStructureData.CreateToFile(RPipelineUnitDefinitionFile);
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
                else
                {
                    RProcessUnits = TreeStructureData.LoadFromStream(RPipelineUnitDefinitionFile.OpenFile());
                }
            }
            {
                StorageFile WPipelineUnitDefinitionFile;
                if (ApplicationStorage.Configuration.CreateFile("WPipelineUnit.tsd", out WPipelineUnitDefinitionFile))
                {
                    WProcessUnits = TreeStructureData.LoadFromStream(WPipelineUnitDefinitionFile.OpenFile());
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
                else
                {
                    WProcessUnits = TreeStructureData.LoadFromStream(WPipelineUnitDefinitionFile.OpenFile());
                }
            }
            {
                //Load CMDOUT Pipelne units.
                StorageFile CMDOUTPipelineUnitsFile;
                if (ApplicationStorage.Configuration.CreateFile("CMDOUTPipelineUnit.tsd", out CMDOUTPipelineUnitsFile))
                {
                    CMDOUTProcessUnits = TreeStructureData.LoadFromStream(CMDOUTPipelineUnitsFile.OpenFile());
                    {
                        //LWMS.Core.dll
                        TreeNode treeNode = new TreeNode();
                        treeNode.Name = "DLL";
                        treeNode.Value = "LWMS.Management.dll";
                        {
                            TreeNode unit = new TreeNode();
                            unit.Name = "ConsoleOutput";
                            unit.Value = "LWMS.Management.ConsoleCmdOutUnit";
                            treeNode.AddChildren(unit);
                        }
                        {
                            TreeNode unit = new TreeNode();
                            unit.Name = "LogOutput";
                            unit.Value = "LWMS.Management.LogCmdOutUnit";
                            treeNode.AddChildren(unit);
                        }
                        CMDOUTProcessUnits.RootNode.AddChildren(treeNode);
                    }
                    CMDOUTProcessUnits.Serialize();
                }
                else
                {
                    CMDOUTProcessUnits = TreeStructureData.LoadFromStream(CMDOUTPipelineUnitsFile.OpenFile());
                }
            }
            {
                //Init these fields.
                _ = MAX_LOG_SIZE;
                _ = LOG_WATCH_INTERVAL;
                _ = WebSiteContentRoot;
                _ = WebSiteModuleStorageRoot;
                //if (l == 0)
                //{

                //}
            }
        }
        public static void LoadConfiguation()
        {
            StorageFile storageFile;
            _ = ApplicationStorage.Configuration.CreateFile("Server.ini", out storageFile);
            ConfigurationData = INILikeData.LoadFromStream(storageFile.OpenFile());
        }

        internal static TreeStructureData RProcessUnits { get; set; }
        internal static TreeStructureData WProcessUnits { get; set; }
        internal static TreeStructureData CMDOUTProcessUnits { get; set; }
        internal static INILikeData ConfigurationData;
        internal static ListData<string> ManageCommandModules;
        public static List<string> GetManageCommandModules(string Auth)
        {
            List<string> l = new();
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                l = new List<string>(ManageCommandModules);
            }, false, true, PermissionID.ReadConfig, PermissionID.ModifyConfig);
            return l;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Auth"></param>
        /// <param name="TSD">0 - R Pipeline, 1 - W Pipeline, 2 - CMDOUT Pipeline</param>
        /// <returns></returns>
        public static List<KeyValuePair<string, string>> ListTSDRoot(string Auth, int TSD)
        {
            List<KeyValuePair<string, string>> l = new();
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                switch (TSD)
                {
                    case 0:
                        foreach (var item in RProcessUnits.RootNode.Children)
                        {
                            l.Add(new(item.Name, item.Value));
                        }
                        break;
                    case 1:
                        foreach (var item in WProcessUnits.RootNode.Children)
                        {
                            l.Add(new(item.Name, item.Value));
                        }
                        break;
                    case 2:
                        foreach (var item in CMDOUTProcessUnits.RootNode.Children)
                        {
                            l.Add(new(item.Name, item.Value));
                        }
                        break;
                    default:
                        break;
                }
            }, false, true, PermissionID.ReadConfig, PermissionID.ModifyConfig);
            return l;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Auth"></param>
        /// <param name="TSD">0 - R Pipeline, 1 - W Pipeline, 2 - CMDOUT Pipeline</param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string, string>> ListTSDChild(string Auth, int TSD, string Name)
        {
            List<KeyValuePair<string, string>> l = new();
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                switch (TSD)
                {
                    case 0:
                        foreach (var item in RProcessUnits.RootNode.Children)
                        {
                            if (item.Name == Name)
                            {
                                foreach (var item2 in item.Children)
                                {
                                    l.Add(new(item2.Name, item2.Value));

                                }
                                break;
                            }
                        }
                        break;
                    case 1:
                        foreach (var item in WProcessUnits.RootNode.Children)
                        {
                            if (item.Name == Name)
                            {
                                foreach (var item2 in item.Children)
                                {
                                    l.Add(new(item2.Name, item2.Value));

                                }
                                break;
                            }
                        }
                        break;
                    case 2:
                        foreach (var item in CMDOUTProcessUnits.RootNode.Children)
                        {
                            if (item.Name == Name)
                            {
                                foreach (var item2 in item.Children)
                                {
                                    l.Add(new(item2.Name, item2.Value));

                                }
                                break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }, false, true, PermissionID.ReadConfig, PermissionID.ModifyConfig);
            return l;
        }
        internal static string _WebModuleStorage = null;
        internal static string _WebSiteContentRoot = null;
        internal static string _DefultPage = null;
        internal static string _Language = null;
        internal static string _Page404 = null;
        internal static bool? _EnableRange = true;
        internal static bool? _LogUA = null;
        internal static int _BUF_LENGTH = 0;
        internal static int _MAX_LOG_SIZE = 0;
        internal static int _LOG_WATCH_INTERVAL = 0;
        public static void Set_BUF_LENGTH_RT(string AuthContext, int VALUE)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () =>
            {
                _BUF_LENGTH = VALUE;
            }, false, true, PermissionID.RTSetBufLength, PermissionID.ModifyRuntimeConfig, PermissionID.ModifyConfig, PermissionID.RuntimeAll);
        }
        public static void Set_WebRoot_RT(string AuthContext, string Path)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () =>
            {
                _WebSiteContentRoot = Path;
            }, false, true, PermissionID.RTWebroot, PermissionID.ModifyRuntimeConfig, PermissionID.ModifyConfig, PermissionID.RuntimeAll);
        }
        internal static List<string> _ListenPrefixes = new List<string>();
        public static void ClearLoadedSettings()
        {
            _WebModuleStorage = null;
            _WebSiteContentRoot = null;
            _DefultPage = null;
            _Page404 = null;
            _BUF_LENGTH = 0;
            _ListenPrefixes = new List<string>();
        }
        public static void RegisterCommandModule(string ContextAuth, string FullPath)
        {

            OperatorAuthentication.AuthedAction(ContextAuth, () =>
            {
                GlobalConfiguration.ManageCommandModules.Add(FullPath);
            }, false, true, PermissionID.UnregisterCmdModule, PermissionID.CmdModuleAll);
        }
        public static void UnregisterCommandModule(string ContextAuth, string Module)
        {
            OperatorAuthentication.AuthedAction(ContextAuth, () =>
            {
                for (int i = 0; i < ManageCommandModules.Count; i++)
                {
                    if (ManageCommandModules[i].EndsWith(Module))
                    {
                        ManageCommandModules.RemoveAt(i);
                        break;
                    }
                }
            }, false, true, PermissionID.UnregisterCmdModule, PermissionID.CmdModuleAll);

        }
        public static void RemovePipeline(string AuthContext, string TYPE, string TARGETDLL)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () =>
            {

                if (TYPE == "R" || TYPE == "/R" || TYPE == "REQUEST")
                {

                    for (int a = 0; a < RProcessUnits.RootNode.Children.Count; a++)
                    {
                        if (RProcessUnits.RootNode.Children[a].Value == TARGETDLL)
                        {
                            RProcessUnits.RootNode.Children.RemoveAt(a);
                            break;
                        }
                    }
                    RProcessUnits.Serialize();
                }
                else
                if (TYPE == "C" || TYPE == "/C" || TYPE == "CMDOUT")
                {
                    for (int a = 0; a < CMDOUTProcessUnits.RootNode.Children.Count; a++)
                    {
                        if (CMDOUTProcessUnits.RootNode.Children[a].Value == TARGETDLL)
                        {
                            CMDOUTProcessUnits.RootNode.Children.RemoveAt(a);
                            break;
                        }
                    }
                    CMDOUTProcessUnits.Serialize();
                }
                else
                if (TYPE == "W" || TYPE == "/W" || TYPE == "WRITE")
                {
                    for (int a = 0; a < WProcessUnits.RootNode.Children.Count; a++)
                    {
                        if (WProcessUnits.RootNode.Children[a].Value == TARGETDLL)
                        {
                            WProcessUnits.RootNode.Children.RemoveAt(a);
                            break;
                        }
                    }
                    WProcessUnits.Serialize();
                }
                Trace.WriteLine(Localization.Language.Query("ManageCmd.Pipeline.Removed", "Removed:{0}", TARGETDLL));
            }, false, false, "Core.Config.RemovePipeline", "Core.Config.AllPipeline");
        }
        public static void UnregisterPipeline(string AuthContext, string TYPE, string TARGETENTRY)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () =>
            {

                bool B = false;
                if (TYPE == "R" || TYPE == "/R" || TYPE == "REQUEST")
                {
                    TYPE = Localization.Language.Query("ManageCmd.Pipeline.Types.R", "Request");
                    foreach (var item in RProcessUnits.RootNode.Children)
                    {
                        for (int a = 0; a < item.Children.Count; a++)
                        {
                            if (item.Children[a].Value == TARGETENTRY)
                            {
                                item.Children.RemoveAt(a);
                                B = true;
                                break;
                            }
                        }
                        if (B == true)
                        {
                            break;
                        }
                    }
                    RProcessUnits.Serialize();
                }
                else if (TYPE == "W" || TYPE == "/W" || TYPE == "WRITE")
                {
                    TYPE = Localization.Language.Query("ManageCmd.Pipeline.Types.W", "Write");
                    foreach (var item in WProcessUnits.RootNode.Children)
                    {
                        for (int a = 0; a < item.Children.Count; a++)
                        {
                            if (item.Children[a].Value == TARGETENTRY)
                            {
                                item.Children.RemoveAt(a);
                                B = true;
                                break;
                            }
                        }
                        if (B == true)
                        {
                            break;
                        }
                    }
                    WProcessUnits.Serialize();
                }
                else if (TYPE == "C" || TYPE == "/C" || TYPE == "CMDOUT")
                {
                    TYPE = Localization.Language.Query("ManageCmd.Pipeline.Types.C", "Command Output");
                    foreach (var item in CMDOUTProcessUnits.RootNode.Children)
                    {
                        for (int a = 0; a < item.Children.Count; a++)
                        {
                            if (item.Children[a].Value == TARGETENTRY)
                            {
                                item.Children.RemoveAt(a);
                                B = true;
                                break;
                            }
                        }
                        if (B == true)
                        {
                            break;
                        }
                    }
                    CMDOUTProcessUnits.Serialize();
                }
                Trace.WriteLine($"Unregistered:{TARGETENTRY} At:{TYPE} pipeline");
                Trace.WriteLine(Localization.Language.Query("ManageCmd.Pipeline.Unregistered", "Unregistered:{0} At:{1} pipeline", TARGETENTRY, TYPE));
            }, false, false, "Core.Config.UnregisterPipeline", "Core.Config.AllPipeline");
        }
        public static void RegisterPipeline(string AuthContext, string TYPE, string DLL, string ENTRY)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () =>
            {
                if (File.Exists(DLL))
                {
                    bool Hit = false;
                    if (TYPE == "W" || TYPE == "/W" || TYPE == "WRITE")
                    {

                        WProcessUnits.RootNode.Children.ForEach((TreeNode item) =>
                        {
                            if (item.Value == DLL)
                            {

                                TreeNode unit = new TreeNode();
                                unit.Name = RandomTool.GetRandomString(8, RandomStringRange.R2);
                                unit.Value = ENTRY;
                                item.AddChildren(unit);
                                Hit = true;
                            }
                        });
                        if (Hit == false)
                        {
                            {
                                TreeNode treeNode = new TreeNode();
                                treeNode.Name = "DLL";
                                treeNode.Value = DLL;
                                {
                                    TreeNode unit = new TreeNode();
                                    unit.Name = RandomTool.GetRandomString(8, RandomStringRange.R2);
                                    unit.Value = ENTRY;
                                    treeNode.AddChildren(unit);
                                }
                                WProcessUnits.RootNode.AddChildren(treeNode);
                            }
                        }
                        WProcessUnits.Serialize();
                    }
                    else if (TYPE == "R" || TYPE == "/R" || TYPE == "REQUEST")
                    {

                        RProcessUnits.RootNode.Children.ForEach((TreeNode item) =>
                        {
                            if (item.Value == DLL)
                            {

                                TreeNode unit = new TreeNode();
                                unit.Name = RandomTool.GetRandomString(8, RandomStringRange.R2);
                                unit.Value = ENTRY;
                                item.AddChildren(unit);
                                Hit = true;
                            }
                        });
                        if (Hit == false)
                        {
                            {
                                TreeNode treeNode = new TreeNode();
                                treeNode.Name = "DLL";
                                treeNode.Value = DLL;
                                {
                                    TreeNode unit = new TreeNode();
                                    unit.Name = RandomTool.GetRandomString(8, RandomStringRange.R2);
                                    unit.Value = ENTRY;
                                    treeNode.AddChildren(unit);
                                }
                                RProcessUnits.RootNode.AddChildren(treeNode);
                            }
                        }
                        RProcessUnits.Serialize();
                    }
                    else if (TYPE == "C" || TYPE == "/C" || TYPE == "CMDOUT")
                    {

                        CMDOUTProcessUnits.RootNode.Children.ForEach((TreeNode item) =>
                        {
                            if (item.Value == DLL)
                            {

                                TreeNode unit = new TreeNode();
                                unit.Name = RandomTool.GetRandomString(8, RandomStringRange.R2);
                                unit.Value = ENTRY;
                                item.AddChildren(unit);
                                Hit = true;
                            }
                        });
                        if (Hit == false)
                        {
                            {
                                TreeNode treeNode = new TreeNode();
                                treeNode.Name = "DLL";
                                treeNode.Value = DLL;
                                {
                                    TreeNode unit = new TreeNode();
                                    unit.Name = RandomTool.GetRandomString(8, RandomStringRange.R2);
                                    unit.Value = ENTRY;
                                    treeNode.AddChildren(unit);
                                }
                                CMDOUTProcessUnits.RootNode.AddChildren(treeNode);
                            }
                        }
                        CMDOUTProcessUnits.Serialize();
                    }
                    else
                    {
                        Trace.WriteLine("Unknown pipeline type:" + TYPE);
                    }
                    Trace.WriteLine($"Registered:{ENTRY}={DLL}");
                }
                else
                {
                    Trace.WriteLine($"Cannot register pipeline unit:{ENTRY}={DLL}");
                }
            }, false, false, "Core.Config.RegisterPipeline", "Core.Config.AllPipeline");

        }
        internal static bool LogUA
        {
            get
            {
                if (_LogUA == null)
                {
                    var value = ConfigurationData.FindValue("LogUA");
                    if (value == null)
                    {
                        _LogUA = false;
                        ConfigurationData.AddValue("LogUA", _LogUA + "", false, true);
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
                if (ConfigurationData != null)
                    ConfigurationData.AddValue("LogUA", _LogUA + "", false, true);

            }
        }
        public static void SetLogUA(string Auth, bool V)
        {
            OperatorAuthentication.AuthedAction(Auth, () => { LogUA = V; }, false, true, PermissionID.ModifyConfig);
        }
        public static bool GetLogUA(string Auth)
        {
            bool v = false;
            OperatorAuthentication.AuthedAction(Auth, () => { v = LogUA; }, false, true, PermissionID.ReadConfig, PermissionID.ModifyConfig);
            return v;
        }
        internal static bool EnableRange
        {
            get
            {
                if (_EnableRange == null)
                {
                    var value = ConfigurationData.FindValue("EnableRange");
                    if (value == null)
                    {
                        _EnableRange = true;
                        ConfigurationData.AddValue("EnableRange", _EnableRange + "", false, true);
                    }
                    else
                    {
                        _EnableRange = bool.Parse(value);
                    }
                }

                return _EnableRange == true ? true : false;
            }
            set
            {
                _EnableRange = value;
                if (ConfigurationData != null)
                    ConfigurationData.AddValue("EnableRange", _EnableRange + "", false, true);

            }
        }
        public static void SetEnableRange(string Auth, bool V)
        {
            OperatorAuthentication.AuthedAction(Auth, () => { EnableRange = V; }, false, true, PermissionID.ModifyConfig);
        }
        public static bool GetEnableRange(string Auth)
        {
            bool v = false;
            OperatorAuthentication.AuthedAction(Auth, () => { v = EnableRange; }, false, true, PermissionID.ReadConfig, PermissionID.ModifyConfig);
            return v;
        }
        internal static int BUF_LENGTH
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
                        Trace.WriteLine(Localization.Language.Query("LWMS.Config.Error.Save", "Cannot save configurations."));

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
        public static int GetBUF_LENGTH(string AuthContext)
        {
            int r = -1;
            OperatorAuthentication.AuthedAction(AuthContext, () => { r = BUF_LENGTH; }, false, true, PermissionID.ReadConfig, PermissionID.ModifyConfig);
            return r;
        }
        public static void SetBUF_LENGTH(string AuthContext, int Size)
        {

            OperatorAuthentication.AuthedAction(AuthContext, () => { BUF_LENGTH = Size; }, false, true, PermissionID.ModifyConfig);
        }
        public static int MAX_LOG_SIZE
        {
            get
            {
                if (_MAX_LOG_SIZE == 0)
                {
                    try
                    {

                        var cs = ConfigurationData.FindValue("MAX_LOG_SIZE");
                        if (cs == null)
                        {
                            _MAX_LOG_SIZE = 10485760;//10 MB per log.
                            ConfigurationData.AddValue("MAX_LOG_SIZE", _MAX_LOG_SIZE + "", AutoSave: true);
                        }
                        else
                        {
                            try
                            {
                                _MAX_LOG_SIZE = int.Parse(cs);
                            }
                            catch (Exception)
                            {
                                _MAX_LOG_SIZE = 10485760;//10 MB per log.
                                ConfigurationData.AddValue("MAX_LOG_SIZE", _MAX_LOG_SIZE + "", AutoSave: true);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Trace.WriteLine(Localization.Language.Query("LWMS.Config.Error.Save", "Cannot save configurations."));

                    }

                }
                LWMSTraceListener._MAX_LOG_SIZE = _MAX_LOG_SIZE;
                return _MAX_LOG_SIZE;
            }
            set
            {
                _MAX_LOG_SIZE = value;
                LWMSTraceListener._MAX_LOG_SIZE = _MAX_LOG_SIZE;
                if (ConfigurationData != null)
                    ConfigurationData.AddValue("MAX_LOG_SIZE", _MAX_LOG_SIZE + "", AutoSave: true);
            }
        }
        public static int LOG_WATCH_INTERVAL
        {
            get
            {
                if (_LOG_WATCH_INTERVAL == 0)
                {
                    try
                    {

                        var cs = ConfigurationData.FindValue("LOG_WATCH_INTERVAL");
                        if (cs == null)
                        {
                            _LOG_WATCH_INTERVAL = 5;//5 ms by default
                            ConfigurationData.AddValue("LOG_WATCH_INTERVAL", _LOG_WATCH_INTERVAL + "", AutoSave: true);
                        }
                        else
                        {
                            try
                            {
                                _LOG_WATCH_INTERVAL = int.Parse(cs);
                            }
                            catch (Exception)
                            {
                                _LOG_WATCH_INTERVAL = 5;//5 ms by default
                                ConfigurationData.AddValue("LOG_WATCH_INTERVAL", _LOG_WATCH_INTERVAL + "", AutoSave: true);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Trace.WriteLine(Localization.Language.Query("LWMS.Config.Error.Save", "Cannot save configurations."));

                    }

                }
                LWMSTraceListener._LOG_WATCH_INTERVAL = _LOG_WATCH_INTERVAL;
                return _MAX_LOG_SIZE;
            }
            set
            {
                _LOG_WATCH_INTERVAL = value;
                LWMSTraceListener._LOG_WATCH_INTERVAL = _LOG_WATCH_INTERVAL;
                if (ConfigurationData != null)
                    ConfigurationData.AddValue("LOG_WATCH_INTERVAL", _LOG_WATCH_INTERVAL + "", AutoSave: true);
            }
        }
        internal static string Page404
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
                        Trace.WriteLine(Localization.Language.Query("LWMS.Config.Error.Save", "Cannot save configurations."));
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
        public static void SetPage404(string AuthContext, string Location)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () => { Page404 = Location; }, false, true, PermissionID.ModifyConfig);
        }
        public static string GetPage404(string AuthContext)
        {
            string l = null;
            OperatorAuthentication.AuthedAction(AuthContext, () => { l = Page404; }, false, true, PermissionID.ReadConfig, PermissionID.ModifyConfig);
            return l;
        }
        public static string Language
        {
            get
            {
                if (_Language == null)
                {
                    try
                    {
                        _Language = ConfigurationData.FindValue("Language");
                        if (_Language == null)
                        {
                            Trace.WriteLine("Generating default language setting.");
                            try
                            {

                                _Language = CultureInfo.CurrentCulture.Name;
                            }
                            catch (Exception)
                            {
                                _Language = "en-US";
                            }
                            ConfigurationData.AddValue("Language", _Language, AutoSave: true);
                        }
                    }
                    catch
                    {
                        Trace.WriteLine(Localization.Language.Query("LWMS.Config.Error.Save", "Cannot save configurations."));
                    }
                    ConfigurationData.Flush();
                }
                return _Language;
            }
            set
            {
                _Language = value;
                if (ConfigurationData != null)
                    ConfigurationData.AddValue("Language", _Language, AutoSave: true);
                if (ConfigurationData != null)
                    ConfigurationData.Flush();
            }
        }

        internal static List<string> ListenPrefixes
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
        public static void RemoveListenPrefixAt(string AuthContext, int Index)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () => { ListenPrefixes.RemoveAt(Index); }, false, true, PermissionID.ModifyConfig);
        }
        public static void RemoveListenPrefix(string AuthContext, string item)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () => { ListenPrefixes.Remove(item); }, false, true, PermissionID.ModifyConfig);
        }
        public static void ClearListenPrefixes(string AuthContext)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () => { ListenPrefixes.Clear(); }, false, true, PermissionID.ModifyConfig);
        }
        public static void SetListenPrefixes(string AuthContext, List<string> ItemList)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () => { ListenPrefixes = ItemList; }, false, true, PermissionID.ModifyConfig);
        }
        public static int GetListenPrefixesCount(string AuthContext)
        {
            int c = -1;
            OperatorAuthentication.AuthedAction(AuthContext, () => { c = ListenPrefixes.Count; }, false, true, PermissionID.ReadConfig, PermissionID.ModifyConfig);
            return c;
        }
        public static IReadOnlyList<string> GetListenPrefixes(string AuthContext)
        {
            IReadOnlyList<string> l = null;
            OperatorAuthentication.AuthedAction(AuthContext, () => { l = ListenPrefixes; }, false, true, PermissionID.ReadConfig, PermissionID.ModifyConfig);
            return l;
        }
        internal static string WebSiteContentRoot
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
                            _WebSiteContentRoot = Path.Combine(ApplicationStorage.BasePath, "webroot");
                            ConfigurationData.AddValue("WebContentRoot", _WebSiteContentRoot, AutoSave: true);
                        }
                    }
                    catch
                    {
                        Trace.WriteLine(Localization.Language.Query("LWMS.Config.Error.Save", "Cannot save configurations."));

                    }
                    ConfigurationData.Flush();
                    try
                    {
                        ApplicationStorage.SetRealWebRoot(TrustedInstaller, _WebSiteContentRoot);

                    }
                    catch (Exception)
                    {
                    }

                }
                return _WebSiteContentRoot;
            }
            set
            {
                _WebSiteContentRoot = value;
                try
                {
                    ApplicationStorage.SetRealWebRoot(TrustedInstaller, _WebSiteContentRoot);
                }
                catch (Exception)
                {
                }
                if (ConfigurationData != null)
                    ConfigurationData.AddValue("WebContentRoot", _WebSiteContentRoot, AutoSave: true);
                if (ConfigurationData != null)
                    ConfigurationData.Flush();
            }
        }
        public static string GetWebSiteContentRoot(string AuthContext)
        {
            string Result = null;
            OperatorAuthentication.AuthedAction(AuthContext, () => { Result = WebSiteContentRoot; }, false, true, PermissionID.ReadConfig, PermissionID.ModifyConfig);
            return Result;
        }
        public static void GetWebSiteContentRoot(string AuthContext, string Value)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () => { WebSiteContentRoot = Value; }, false, true, PermissionID.ModifyConfig);
        }
        internal static string WebSiteModuleStorageRoot
        {
            get
            {
                if (_WebModuleStorage == null)
                {
                    try
                    {
                        _WebModuleStorage = ConfigurationData.FindValue("ModuleStorageRoot");
                        if (_WebModuleStorage == null)
                        {
                            _WebModuleStorage = Path.Combine(ApplicationStorage.BasePath, "webmodule");
                            ConfigurationData.AddValue("ModuleStorageRoot", _WebModuleStorage, AutoSave: true);
                        }
                    }
                    catch
                    {
                        Trace.WriteLine(Localization.Language.Query("LWMS.Config.Error.Save", "Cannot save configurations."));

                    }
                    ConfigurationData.Flush(); try
                    {
                        ApplicationStorage.SetRealModuleRoot(TrustedInstaller, _WebModuleStorage);
                    }
                    catch (Exception)
                    {
                    }

                }
                return _WebModuleStorage;
            }
            set
            {
                _WebModuleStorage = value; try
                {
                    ApplicationStorage.SetRealModuleRoot(TrustedInstaller, _WebModuleStorage);
                }
                catch (Exception)
                {
                }
                if (ConfigurationData != null)
                    ConfigurationData.AddValue("ModuleStorageRoot", _WebModuleStorage, AutoSave: true);
                if (ConfigurationData != null)
                    ConfigurationData.Flush();
            }
        }
        public static string GetWebSiteModuleStorageRoot(string AuthContext)
        {
            string r = null;
            OperatorAuthentication.AuthedAction(AuthContext, () => { r = WebSiteModuleStorageRoot; }, false, true, PermissionID.ReadConfig_WebSiteModuleStorageRoot);
            return r;
        }
        public static void SetWebSiteModuleStorageRoot(string AuthContext, string Value)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () => { WebSiteModuleStorageRoot = Value; }, false, true, PermissionID.ModifyConfig);
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
                        Trace.WriteLine(Localization.Language.Query("LWMS.Config.Error.Save", "Cannot save configurations."));

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

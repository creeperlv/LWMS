using CLUNL.Data.Layer0;
using CLUNL.Utilities;
using LWMS.Core;
using LWMS.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LWMS.Management.Commands
{
    public class PipelineManager : IManageCommand
    {
        public string CommandName => "ManagePipeline";
        List<string> vs = new List<string>();
        public PipelineManager()
        {
            vs.Add("Pipeline");
            vs.Add("ppl");
        }
        public List<string> Alias { get => vs; }

        public int Version => 2;

//        static string HelpString = @"Usage:
//ManagePipeline <Operation> <PipelineType> [<DllFile> <EnrtyPoint>]

//Aliases:Pipeline,ppl

//Operations:

//    REG|REGISTER
//        Register a pipeline unit.

//    UNREG|UNREGISTER
//        Unregister a pipeline unit.

//    RM|REMOVE
//        Remove a registered dll file.

//Pipeline Types:
//    R,/R,Request    Request process pipeline
//    W,/W,Write      Write Routed Stream Pipeline.
//    C,/C,CmdOut     Command Output Pipeline.

//Example:

//    ppl reg r LWMS.RPipelineUnits.dll LWMS.RPipelineUnits.ProcessedStaticPages
//    ppl rm r LWMS.RPipelineUnits.dll
//";

        public static void PrintHelp()
        {
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Usage", "Usage:"));
            Output.WriteLine("");
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.Usage", "ManagePipeline <Operation> <PipelineType> [<DllFile> <EnrtyPoint>]"));
            Output.WriteLine("");
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.Aliases", "Aliasese:Pipeline,ppl"));
            Output.WriteLine("");
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Operations", "Operations:"));
            Output.WriteLine("");
            Output.WriteLine("\tREG|REGISTER");
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.Register", "\t\tRegister a pipeline unit."));
            Output.WriteLine("");
            Output.WriteLine("\tUNREG|UNREGISTER");
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.Register", "\t\tUnregister a pipeline unit."));
            Output.WriteLine("");
            Output.WriteLine("\tRM|REMOVE");
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.Remove", "\t\tRemove a registered dll file."));
            Output.WriteLine("");
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.PipelineTypes", "Pipeline Types:"));
            Output.WriteLine("");
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.PipelineTypes.R",      "\tR,/R,Request     Request process pipeline"));
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.PipelineTypes.W",      "\tW,/W,Write       Write process pipeline"));
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.PipelineTypes.CmdOut", "\tC,/C,CmdOut      Command Out pipeline"));
            Output.WriteLine("");
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Example", "Example:"));
            Output.WriteLine("");
            Output.WriteLine("\tppl reg r LWMS.RPipelineUnits.dll LWMS.RPipelineUnits.ProcessedStaticPages");
            Output.WriteLine("\tppl rm r LWMS.RPipelineUnits.dll");
        }
        public void Invoke(params CommandPack[] args)
        {
            if (args.Length == 0)
            {
                Output.SetForegroundColor(ConsoleColor.Yellow);
                Output.WriteLine(Language.Query("ManageCmd.Help.Config.Error.NoOperation", "Please specify an operation."));
                Output.ResetColor();
                PrintHelp();
            }
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToUpper() == "REG" || args[i].ToUpper() == "REGISTER")
                {
                    string TYPE = args[i + 1].ToUpper();
                    string DLL = args[i + 2];
                    string ENTRY = args[i + 3];

                    i += 3;
                    if (File.Exists(DLL))
                    {
                        bool Hit = false;
                        if (TYPE == "W" || TYPE == "/W" || TYPE == "WRITE")
                        {

                            Configuration.WProcessUnits.RootNode.Children.ForEach((TreeNode item) =>
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
                                    Configuration.WProcessUnits.RootNode.AddChildren(treeNode);
                                }
                            }
                            Configuration.WProcessUnits.Serialize();
                        }
                        else if (TYPE == "R" || TYPE == "/R" || TYPE == "REQUEST")
                        {

                            Configuration.RProcessUnits.RootNode.Children.ForEach((TreeNode item) =>
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
                                    Configuration.RProcessUnits.RootNode.AddChildren(treeNode);
                                }
                            }
                            Configuration.RProcessUnits.Serialize();
                        }
                        else if (TYPE == "C" || TYPE == "/C" || TYPE == "CMDOUT")
                        {

                            Configuration.CMDOUTProcessUnits.RootNode.Children.ForEach((TreeNode item) =>
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
                                    Configuration.CMDOUTProcessUnits.RootNode.AddChildren(treeNode);
                                }
                            }
                            Configuration.CMDOUTProcessUnits.Serialize();
                        }
                        else
                        {
                            Output.WriteLine("Unknown pipeline type:" + TYPE);
                        }
                        Output.WriteLine($"Registered:{ENTRY}={DLL}");
                    }
                    else
                    {
                        Output.WriteLine($"Cannot register pipeline unit:{ENTRY}={DLL}");
                    }
                }
                else if (args[i].ToUpper() == "UNREG" || args[i].ToUpper() == "UNREGISTER")
                {
                    string TYPE = args[i + 1].ToUpper();
                    string TARGETENTRY = args[i + 2];
                    i += 2;
                    bool B = false;
                    if (TYPE == "R" || TYPE == "/R" || TYPE == "REQUEST")
                    {
                        TYPE = Language.Query("ManageCmd.Pipeline.Types.R","Request");
                        foreach (var item in Configuration.RProcessUnits.RootNode.Children)
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
                        Configuration.RProcessUnits.Serialize();
                    }
                    else if (TYPE == "W" || TYPE == "/W" || TYPE == "WRITE")
                    {
                        TYPE = Language.Query("ManageCmd.Pipeline.Types.W","Write");
                        foreach (var item in Configuration.WProcessUnits.RootNode.Children)
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
                        Configuration.WProcessUnits.Serialize();
                    }
                    else if (TYPE == "C" || TYPE == "/C" || TYPE == "CMDOUT")
                    {
                        TYPE = Language.Query("ManageCmd.Pipeline.Types.C","Command Output");
                        foreach (var item in Configuration.CMDOUTProcessUnits.RootNode.Children)
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
                        Configuration.CMDOUTProcessUnits.Serialize();
                    }
                    Output.WriteLine($"Unregistered:{TARGETENTRY} At:{TYPE} pipeline");
                    Output.WriteLine(Language.Query("ManageCmd.Pipeline.Unregistered","Unregistered:{0} At:{1} pipeline",TARGETENTRY,TYPE));
                }
                else if (args[i].ToUpper() == "REMOVE" || args[i].ToUpper() == "RM")
                {
                    string TYPE = args[i + 1].ToUpper();
                    string TARGETDLL = args[i + 2];
                    i += 2;

                    if (TYPE == "R" || TYPE == "/R" || TYPE == "REQUEST")
                    {

                        for (int a = 0; a < Configuration.RProcessUnits.RootNode.Children.Count; a++)
                        {
                            if (Configuration.RProcessUnits.RootNode.Children[a].Value == TARGETDLL)
                            {
                                Configuration.RProcessUnits.RootNode.Children.RemoveAt(a);
                                break;
                            }
                        }
                    }
                    else
                    if (TYPE == "C" || TYPE == "/C" || TYPE == "CMDOUT")
                    {
                        for (int a = 0; a < Configuration.CMDOUTProcessUnits.RootNode.Children.Count; a++)
                        {
                            if (Configuration.CMDOUTProcessUnits.RootNode.Children[a].Value == TARGETDLL)
                            {
                                Configuration.CMDOUTProcessUnits.RootNode.Children.RemoveAt(a);
                                break;
                            }
                        }
                    }
                    else
                    if (TYPE == "W" || TYPE == "/W" || TYPE == "WRITE")
                    {
                        for (int a = 0; a < Configuration.WProcessUnits.RootNode.Children.Count; a++)
                        {
                            if (Configuration.WProcessUnits.RootNode.Children[a].Value == TARGETDLL)
                            {
                                Configuration.WProcessUnits.RootNode.Children.RemoveAt(a);
                                break;
                            }
                        }
                    }
                    Configuration.RProcessUnits.Serialize();
                    Output.WriteLine(Language.Query("ManageCmd.Pipeline.Removed","Removed:{0}", TARGETDLL));
                }
                else if (args[i].ToUpper() == "H" || args[i].ToUpper() == "HELP" || args[i].ToUpper() == "--H" || args[i].ToUpper() == "-H" || args[i].ToUpper() == "?" || args[i].ToUpper() == "-?" || args[i].ToUpper() == "--?")
                {
                    PrintHelp();
                }
                else
                {
                    Output.SetForegroundColor(ConsoleColor.Yellow);
                    Output.WriteLine(Language.Query("ManageCmd.Universal.UnknownOperation","Unknown operation:{0}" , args[i]));
                    Output.ResetColor();
                    PrintHelp();
                }
            }
        }
    }
}

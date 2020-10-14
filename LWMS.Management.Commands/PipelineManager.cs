using CLUNL.Data.Layer0;
using CLUNL.Utilities;
using LWMS.Core;
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

        public int Version => throw new NotImplementedException();

        public void Invoke(params CommandPack[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToUpper() == "REG" || args[i].ToUpper() == "REGISTER")
                {
                    string DLL = args[i + 1];
                    string ENTRY = args[i + 2];
                    i += 2;
                    if (File.Exists(DLL))
                    {
                        bool Hit = false;
                        Configuration.ProcessUnits.RootNode.Children.ForEach((TreeNode item) =>
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
                                Configuration.ProcessUnits.RootNode.AddChildren(treeNode);
                            }
                        }
                        Configuration.ProcessUnits.Serialize();
                    }
                    else
                    {
                        Trace.WriteLine($"Cannot register pipeline unit:{ENTRY}={DLL}");
                    }
                }else if (args[i].ToUpper() == "UNREG" || args[i].ToUpper() == "UNREGISTER")
                {
                    string TARGETENTRY = args[i + 1];
                    i++;
                    bool B=false;
                    foreach (var item in Configuration.ProcessUnits.RootNode.Children)
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
                    Configuration.ProcessUnits.Serialize();
                }else if (args[i].ToUpper() == "REMOVE" || args[i].ToUpper() == "RM")
                {
                    string TARGETDLL = args[i + 1];
                    i++;
                    for (int a = 0; a < Configuration.ProcessUnits.RootNode.Children.Count; a++)
                    {
                        if (Configuration.ProcessUnits.RootNode.Children[a].Value == TARGETDLL)
                        {
                            Configuration.ProcessUnits.RootNode.Children.RemoveAt(a);
                            break;
                        }
                    }
                    Configuration.ProcessUnits.Serialize();
                }
            }
        }
    }
}

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
                                //LWMS.Core.dll
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
                }
            }
        }
    }
}

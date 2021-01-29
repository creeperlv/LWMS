using CLUNL.Data.Layer0;
using CLUNL.Utilities;
using LWMS.Core;
using LWMS.Core.Configuration;
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
        public void Invoke(string AuthContext, params CommandPack[] args)
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
                    string TYPE = args[i + 1].PackTotal.ToUpper();
                    string DLL = args[i + 2].PackTotal;
                    string ENTRY = args[i + 3].PackTotal;
                    GlobalConfiguration.RegisterPipeline(AuthContext, TYPE, DLL, ENTRY);
                    i += 3;
                }
                else if (args[i].ToUpper() == "UNREG" || args[i].ToUpper() == "UNREGISTER")
                {
                    string TYPE = args[i + 1].ToUpper();
                    string TARGETENTRY = args[i + 2];
                    i += 2;
                    GlobalConfiguration.UnregisterPipeline(AuthContext, TYPE, TARGETENTRY);
                }
                else if (args[i].ToUpper() == "REMOVE" || args[i].ToUpper() == "RM")
                {
                    string TYPE = args[i + 1].ToUpper();
                    string TARGETDLL = args[i + 2];
                    i += 2;
                    GlobalConfiguration.RemovePipeline(AuthContext, TYPE, TARGETDLL);
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

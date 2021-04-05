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

        public int Version => 4;

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

        public static void PrintHelp(string AuthContext)
        {
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Usage", "Usage:"), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.Usage", "ManagePipeline <Operation> <PipelineType> [<DllFile> <EnrtyPoint>]"), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.Aliases", "Aliasese:Pipeline,ppl"), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Operations", "Operations:"), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine("\tREG|REGISTER", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.Register", "\t\tRegister a pipeline unit."), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine("\tUNREG|UNREGISTER", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.Register", "\t\tUnregister a pipeline unit."), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine("\tRM|REMOVE", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.Remove", "\t\tRemove a registered dll file."), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.PipelineTypes", "Pipeline Types:"), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.PipelineTypes.R",      "\tR,/R,Request     Request process pipeline"), AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.PipelineTypes.W",      "\tW,/W,Write       Write process pipeline"), AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Pipeline.PipelineTypes.CmdOut", "\tC,/C,CmdOut      Command Out pipeline"), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Example", "Example:"), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine("\tppl reg r LWMS.RPipelineUnits.dll LWMS.RPipelineUnits.ProcessedStaticPages", AuthContext);
            Output.WriteLine("\tppl rm r LWMS.RPipelineUnits.dll", AuthContext);
        }
        public void Invoke(string AuthContext, params CommandPack[] args)
        {
            if (args.Length == 0)
            {
                Output.SetForegroundColor(ConsoleColor.Yellow, AuthContext);
                Output.WriteLine(Language.Query("ManageCmd.Help.Config.Error.NoOperation", "Please specify an operation."), AuthContext);
                Output.ResetColor(AuthContext);
                PrintHelp(AuthContext);
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
                    PrintHelp(AuthContext);
                }
                else
                {
                    Output.SetForegroundColor(ConsoleColor.Yellow, AuthContext);
                    Output.WriteLine(Language.Query("ManageCmd.Universal.UnknownOperation","Unknown operation:{0}" , args[i]), AuthContext);
                    Output.ResetColor(AuthContext);
                    PrintHelp(AuthContext);
                }
            }
        }
    }
}

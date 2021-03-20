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
    public class CommandManager : IManageCommand
    {
        public string CommandName => "ManageCommand";
        public int Version => 4;

        public List<string> Alias => new List<string>(new string[] { "commands", "cmds" });

        public void Invoke(string AuthContext, params CommandPack[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].PackTotal.ToUpper() == "REGISTER" || args[0].PackTotal.ToUpper() == "REG")
                {
                    if (args.Length == 1)
                    {
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.Register.Error.0", "You must specify module to register."));
                    }
                    FileInfo target = new FileInfo(args[1].PackTotal);
                    if (target.Exists == false)
                    {
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.Register.Error.1", "Target Module not found:{0}", target.FullName));
                    }
                    else
                    {
                        if (ServerController.Register(AuthContext,target.FullName) == true)
                        {
                            GlobalConfiguration.RegisterCommandModule(AuthContext,target.FullName);
                        }
                        else
                        {
                            Output.WriteLine(Language.Query("ManageCmd.CmdMgr.Register.Error.2", "Cannot register target module: Not a validate module."));
                        }
                    }
                }
                else if (args[0].PackTotal.ToUpper() == "UNREGISTER" || args[0].PackTotal.ToUpper() == "UNREG")
                {

                    if (args.Length == 1)
                    {
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.Unregister.Error.0", "You must specify module to unregister."));
                    }
                    GlobalConfiguration.UnregisterCommandModule(AuthContext, args[1]);
                }
                else if (args[0].PackTotal.ToUpper() == "LS" || args[0].PackTotal.ToUpper() == "LIST")
                {
                    foreach (var item in ServerController.ManageCommands)
                    {
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.List.Type", "Type: {0}", item.Value.TargetObject.ToString()));
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.List.Command", "\t\tCommand: {0} ", item.Key));
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.List.Dll", "\t\tDLL: {0} ", item.Value.TargetObject.GetType().Assembly.Location));
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.List.Version", "\t\tVersion: {0} ", (item.Value.TargetObject as IManageCommand).Version.ToString()));
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (string alias in (item.Value.TargetObject as IManageCommand).Alias)
                        {
                            stringBuilder.Append(" ");
                            stringBuilder.Append(alias);
                        }
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.List.Aliases", "\t\tAliases: {0} ", stringBuilder.ToString()));
                    }
                }
            }
            else
            {
                OutputHelp();
            }
        }
        public void OutputHelp()
        {
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Usage", "Usage:"));
            Output.WriteLine(Language.Query("ManageCmd.Help.CmdMgr.Usage", "ManageCommand <Operations> [Option1] [Option2] ..."));
            Output.WriteLine("");
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Operations", "Operations:"));
            Output.WriteLine("\tRegister <Path-to-DLL>");
            Output.WriteLine(Language.Query("ManageCmd.Help.CmdMgr.Register", "\t\tRegister a dll to find all availiable commands."));
            Output.WriteLine("");
            Output.WriteLine("\tUnregister <Path-to-DLL>");
            Output.WriteLine(Language.Query("ManageCmd.Help.CmdMgr.Unregister", "\t\tUnregister a dll, need restart to effect."));
            Output.WriteLine("");
            Output.WriteLine("\tLs/List");
            Output.WriteLine(Language.Query("ManageCmd.Help.CmdMgr.List", "\t\tList all commands, their assembly files and aliases."));
        }
    }
}

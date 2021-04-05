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
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.Register.Error.0", "You must specify module to register."), AuthContext);
                    }
                    FileInfo target = new FileInfo(args[1].PackTotal);
                    if (target.Exists == false)
                    {
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.Register.Error.1", "Target Module not found:{0}", target.FullName), AuthContext);
                    }
                    else
                    {
                        if (ServerController.Register(AuthContext,target.FullName) == true)
                        {
                            GlobalConfiguration.RegisterCommandModule(AuthContext,target.FullName);
                        }
                        else
                        {
                            Output.WriteLine(Language.Query("ManageCmd.CmdMgr.Register.Error.2", "Cannot register target module: Not a validate module."), AuthContext);
                        }
                    }
                }
                else if (args[0].PackTotal.ToUpper() == "UNREGISTER" || args[0].PackTotal.ToUpper() == "UNREG")
                {

                    if (args.Length == 1)
                    {
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.Unregister.Error.0", "You must specify module to unregister."), AuthContext);
                    }
                    GlobalConfiguration.UnregisterCommandModule(AuthContext, args[1]);
                }
                else if (args[0].PackTotal.ToUpper() == "LS" || args[0].PackTotal.ToUpper() == "LIST")
                {
                    foreach (var item in ServerController.ManageCommands)
                    {
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.List.Type", "Type: {0}", item.Value.TargetObject.ToString()), AuthContext);
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.List.Command", "\t\tCommand: {0} ", item.Key), AuthContext);
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.List.Dll", "\t\tDLL: {0} ", item.Value.TargetObject.GetType().Assembly.Location), AuthContext);
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.List.Version", "\t\tVersion: {0} ", (item.Value.TargetObject as IManageCommand).Version.ToString()), AuthContext);
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (string alias in (item.Value.TargetObject as IManageCommand).Alias)
                        {
                            stringBuilder.Append(" ");
                            stringBuilder.Append(alias);
                        }
                        Output.WriteLine(Language.Query("ManageCmd.CmdMgr.List.Aliases", "\t\tAliases: {0} ", stringBuilder.ToString()), AuthContext);
                    }
                }
            }
            else
            {
                OutputHelp(AuthContext);
            }
        }
        public void OutputHelp(string AuthContext)
        {
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Usage", "Usage:"), AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.CmdMgr.Usage", "ManageCommand <Operations> [Option1] [Option2] ..."), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Operations", "Operations:"), AuthContext);
            Output.WriteLine("\tRegister <Path-to-DLL>", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.CmdMgr.Register", "\t\tRegister a dll to find all availiable commands."), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine("\tUnregister <Path-to-DLL>", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.CmdMgr.Unregister", "\t\tUnregister a dll, need restart to effect."), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine("\tLs/List", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.CmdMgr.List", "\t\tList all commands, their assembly files and aliases."), AuthContext);
        }
    }
}

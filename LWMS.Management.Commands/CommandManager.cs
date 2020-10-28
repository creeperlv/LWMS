using LWMS.Core;
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
        public int Version => 2;

        public List<string> Alias => new List<string>(new string[] { "commands", "cmds" });

        public void Invoke(params CommandPack[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].PackTotal.ToUpper() == "REGISTER" || args[0].PackTotal.ToUpper() == "REG")
                {
                    if (args.Length == 1)
                    {
                        Output.WriteLine("You must specify module to register");
                    }
                    FileInfo target = new FileInfo(args[1].PackTotal);
                    if (target.Exists == false)
                    {
                        Output.WriteLine("Target Module not found:" + target);
                    }
                    else
                    {
                        if (ServerController.Register(target.FullName) == true)
                        {
                            Configuration.ManageCommandModules.Add(target.FullName);
                        }
                        else
                        {
                            Output.WriteLine("Cannot register target module: Not a validate module.");
                        }
                    }
                }
                else if(args[0].PackTotal.ToUpper() == "UNREGISTER" || args[0].PackTotal.ToUpper() == "UNREG")
                {

                    if (args.Length == 1)
                    {
                        Output.WriteLine("You must specify module to unregister");
                    }

                    for (int i = 0; i < Configuration.ManageCommandModules.Count; i++)
                    {
                        if (Configuration.ManageCommandModules[i].EndsWith(args[1])){
                            Configuration.ManageCommandModules.RemoveAt(i);
                            break;
                        }
                    }
                }else if (args[0].PackTotal.ToUpper() == "LS"|| args[0].PackTotal.ToUpper() == "LIST"){
                    foreach (var item in ServerController.ManageCommands)
                    {
                        Output.WriteLine("Type:"+item.Value);
                        Output.WriteLine($"\t\tCommand:{item.Key}");
                        Output.WriteLine($"\t\tDLL:{item.Value.GetType().Assembly.Location}");
                        Output.WriteLine($"\t\tVersion:{item.Value.Version}");
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (string alias in item.Value.Alias)
                        {
                            stringBuilder.Append(" ");
                            stringBuilder.Append(alias);
                        }
                        Output.WriteLine($"\t\tAliases:{stringBuilder}");
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
            Output.WriteLine("Usage:");
            Output.WriteLine("ManageCommand <Operations> [Option1] [Option2] ...");
            Output.WriteLine("");
            Output.WriteLine("Operations:");
            Output.WriteLine("\tRegister <Path-to-DLL>");
            Output.WriteLine("\t\tRegister a dll to find all availiable commands.");
            Output.WriteLine("\tUnregister <Path-to-DLL>");
            Output.WriteLine("\t\tUnregister a dll, need restart to effect.");
            Output.WriteLine("\tLs/List");
            Output.WriteLine("\t\tList all commands, their assembly files and aliases.");
        }
    }
}

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
        public int Version => 1;

        public List<string> Alias => new List<string>(new string[] { "commands", "cmds" });

        public void Invoke(params CommandPack[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].PackTotal.ToUpper() == "REGISTER" || args[0].PackTotal.ToUpper() == "REG")
                {
                    if (args.Length == 1)
                    {
                        Trace.WriteLine("You must specify module to register");
                    }
                    FileInfo target = new FileInfo(args[1].PackTotal);
                    if (target.Exists == false)
                    {
                        Trace.WriteLine("Target Module not found:" + target);
                    }
                    else
                    {
                        if (ServerController.Register(target.FullName) == true)
                        {
                            Configuration.ManageCommandModules.Add(target.FullName);
                        }
                        else
                        {
                            Trace.WriteLine("Cannot register target module: Not a validate module.");
                        }
                    }
                }
                else if(args[0].PackTotal.ToUpper() == "UNREGISTER" || args[0].PackTotal.ToUpper() == "UNREG")
                {

                    if (args.Length == 1)
                    {
                        Trace.WriteLine("You must specify module to unregister");
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
                        Trace.WriteLine("Type:"+item.Value);
                        Trace.WriteLine($"\t\tCommand:{item.Key},DLL={item.Value.GetType().Assembly.Location}");
                        Trace.WriteLine($"\t\tVersion:{item.Value.Version}");
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (string alias in item.Value.Alias)
                        {
                            stringBuilder.Append(" ");
                            stringBuilder.Append(alias);
                        }
                        Trace.WriteLine($"\t\tAliases:{stringBuilder}");
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
            Trace.WriteLine("Usage:");
            Trace.WriteLine("ManageCommand <Operations> [Option1] [Option2] ...");
            Trace.WriteLine("");
            Trace.WriteLine("Operations:");
            Trace.WriteLine("\tRegister <Path-to-DLL>");
            Trace.WriteLine("\t\tRegister a dll to find all availiable commands.");
            Trace.WriteLine("\tUnregister <Path-to-DLL>");
            Trace.WriteLine("\t\tUnregister a dll, need restart to effect.");
            Trace.WriteLine("\tLs/List");
            Trace.WriteLine("\t\tList all commands, their assembly files and aliases.");
        }
    }
}

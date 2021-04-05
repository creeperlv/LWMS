using LWMS.Core;
using LWMS.Core.Authentication;
using LWMS.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace LWMS.Management.Commands
{
    public class View : IManageCommand
    {
        public string CommandName => "View";

        public List<string> Alias => new List<string>();

        public int Version => 2;

        public static void PrintHelp(string AuthContext)
        {
            Output.WriteLine("Usage:", AuthContext);
            Output.WriteLine("\tView <Info-To-View>", AuthContext);
            Output.WriteLine("Info that can view:", AuthContext);
            Output.WriteLine("\tWebroot", AuthContext);
            Output.WriteLine("\t\tAbsolute Path of webroot (Using 'DirectoryInfo.FullName')", AuthContext);
            Output.WriteLine("\tWebrootDir", AuthContext);
            Output.WriteLine("\t\tEnumerate all folders and files in webroot (will not show the entire folder tree).", AuthContext);
            Output.WriteLine("\tUsingMem", AuthContext);
            Output.WriteLine("\t\tAllocated Memory to LWMS (Using 'Process.WorkingSet64')", AuthContext);
        }

        public void Invoke(string AuthContext,params CommandPack[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp(AuthContext);
                return;
            }
            OperatorAuthentication.AuthedAction(AuthContext, () => {
                foreach (var item in args)
                {
                    if (item.PackTotal.ToUpper() == "WEBROOT")
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(GlobalConfiguration.GetWebSiteContentRoot(AuthContext));
                        Output.WriteLine(directoryInfo.FullName, AuthContext);
                    }
                    else
                    if (item.PackTotal.ToUpper() == "WEBROOTDIR")
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(GlobalConfiguration.GetWebSiteContentRoot(AuthContext));
                        foreach (var dir in directoryInfo.EnumerateDirectories())
                        {
                            Output.WriteLine("D:" + dir.FullName, AuthContext);
                        }
                        foreach (var file in directoryInfo.EnumerateFiles())
                        {
                            Output.WriteLine("F:" + file.FullName, AuthContext);
                        }
                    }
                    else if (item.ToUpper() == "USINGMEM")
                    {
                        var prop = Process.GetCurrentProcess();
                        Output.WriteLine("Using Memory(KB):" + prop.WorkingSet64 / 1024.0, AuthContext);
                    }
                }
            }, false, true, "Basic.ViewRuntimeInfo");
        }
    }
}

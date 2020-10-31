using LWMS.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LWMS.Management.Commands
{
    public class View : IManageCommand
    {
        public string CommandName => "View";

        public List<string> Alias => new List<string>();

        public int Version => 1;

        public static void PrintHelp()
        {
            Output.WriteLine("Usage:");
            Output.WriteLine("\tView <Info-To-View>");
            Output.WriteLine("Info that can view:");
            Output.WriteLine("\tWebroot");
            Output.WriteLine("\t\tAbsolute Path of webroot (Using 'DirectoryInfo.FullName')");
            Output.WriteLine("\tWebrootDir");
            Output.WriteLine("\t\tEnumerate all folders and files in webroot (will not show the entire folder tree).");
            Output.WriteLine("\tUsingMem");
            Output.WriteLine("\t\tAllocated Memory to LWMS (Using 'Process.WorkingSet64')");
        }

        public void Invoke(params CommandPack[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }
            foreach (var item in args)
            {
                if (item.PackTotal.ToUpper() == "WEBROOT")
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.WebSiteContentRoot);
                    Output.WriteLine(directoryInfo.FullName);
                }
                else
                if (item.PackTotal.ToUpper() == "WEBROOTDIR")
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.WebSiteContentRoot);
                    foreach (var dir in directoryInfo.EnumerateDirectories())
                    {
                        Output.WriteLine("D:"+dir.FullName);
                    }
                    foreach (var file in directoryInfo.EnumerateFiles())
                    {
                        Output.WriteLine("F:"+file.FullName);
                    }
                }else if (item.ToUpper() == "USINGMEM")
                {
                    var prop = Process.GetCurrentProcess();
                    Output.WriteLine("Using Memory(KB):" + prop.WorkingSet64/1024.0);
                }
            }
        }
    }
}

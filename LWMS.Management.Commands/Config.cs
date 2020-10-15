using System;
using System.Collections.Generic;
using System.Text;
using LWMS.Core;
using System.Diagnostics;

namespace LWMS.Management.Commands
{
    public class Config : IManageCommand
    {
        public string CommandName => "Config";

        public List<string> Alias =>new List<string>();

        public int Version => 1;

        public void Invoke(params CommandPack[] args)
        {
            if (args[0].ToUpper() == "RELEASE")
            {
                Configuration.ConfigurationData.Dispose();
                Configuration.ConfigurationData = null;
                Trace.WriteLine("Configuration file is released and changes will not be saved.");
            }
            else if (args[0].ToUpper() == "RESUME")
            {
                Configuration.LoadConfiguation();
                Configuration.ClearLoadedSettings();
                Trace.WriteLine("Resume");
                Trace.WriteLine("Configuration changes will be automatically saved now.");
            }else if (args[0].ToUpper() == "SET")
            {
                if (args.Length >= 3)
                {

                }
                else
                {
                    Trace.WriteLine("arguments does not match: Config set <key> <value>");
                }
            }
        }
    }
}

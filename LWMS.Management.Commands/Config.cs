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

        public List<string> Alias => new List<string>();

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
            }
            else if (args[0].ToUpper() == "RELOAD")
            {
                Configuration.LoadConfiguation();
                Configuration.ClearLoadedSettings();
            }
            else if (args[0].ToUpper() == "SET")
            {
                if (args.Length >= 3)
                {
                    string setitem = args[1].ToUpper();
                    if (setitem == "BUF_LENGTH")
                    {
                        Configuration.BUF_LENGTH = int.Parse(args[2]);
                    }
                    else if (setitem == "WEBROOT" || setitem == "WEBSITEROOT" || setitem == "WEBSITECONTENTROOT" || setitem == "WEBCONTENTROOT" || setitem == "CONTENTROOT")
                    {
                        Configuration.WebSiteContentRoot = args[2];
                    }
                    else if (setitem == "DEFAULTPAGE")
                    {
                        Configuration.DefaultPage = args[2];
                    }
                    else if (setitem == "404PAGE")
                    {
                        Configuration.Page404 = args[2];
                    }
                }
                else
                {
                    Trace.WriteLine("arguments does not match: Config set <key> <value>");
                }
            }
            else if (args[0].ToUpper() == "ADD")
            {
                if (args.Length >= 3)
                {
                    string setitem = args[1].ToUpper();
                    if (setitem == "LISTENPREFIX" || setitem == "LISTEN")
                    {
                        var prefixes = Configuration.ListenPrefixes;
                        prefixes.Add(args[2]);
                        Configuration.ListenPrefixes = prefixes;
                    }
                }
                else
                {
                    Trace.WriteLine("arguments does not match: Config add <key> <value>");
                }
            }
            else if (args[0].ToUpper() == "RM" ||args[0].ToUpper() == "REMOVE")
            {

                if (args.Length >= 3)
                {
                    string setitem = args[1].ToUpper();
                    if (setitem == "LISTENPREFIX" || setitem == "LISTEN")
                    {
                        try
                        {

                            var prefixes = Configuration.ListenPrefixes;
                            prefixes.Remove(args[2]);
                            Configuration.ListenPrefixes = prefixes;

                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else
                {
                    Trace.WriteLine("arguments does not match: Config add <key> <value>");
                }
            }
        }
    }
}

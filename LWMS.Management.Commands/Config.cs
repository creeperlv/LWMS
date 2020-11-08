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

        public int Version => 2;

        public static void OutputHelp()
        {
            Output.WriteLine("Usage:");
            Output.WriteLine("");
            Output.WriteLine("\tConfig <Operation> [<Key> <Value>]");
            Output.WriteLine("");
            Output.WriteLine("Operations:");
            Output.WriteLine("");
            Output.WriteLine("\tRlease");
            Output.WriteLine("\t\tStop writing changes to settings file, and settings file will be released in the same tim. It means settings file will be editable from other application.");
            Output.WriteLine("\tResume");
            Output.WriteLine("\t\tContinue writing changes to settings file, changes during the release will not be saved.");
            Output.WriteLine("\tReload");
            Output.WriteLine("\t\tReload settings.");
            Output.WriteLine("\tSet");
            Output.WriteLine("\t\tSet a value to target key.");
            Output.WriteLine("\tAdd");
            Output.WriteLine("\t\tAdd a value to target collection whose name is given Key.");
            Output.WriteLine("\tRemove,Rm");
            Output.WriteLine("\t\tRemove a value to target collection whose name is given Key.");
        }

        public void Invoke(params CommandPack[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].ToUpper() == "RELEASE")
                {
                    Configuration.ConfigurationData.Dispose();
                    Configuration.ConfigurationData = null;
                    Output.WriteLine("Configuration file is released and changes will not be saved.");
                }
                else if (args[0].ToUpper() == "RESUME")
                {
                    Configuration.LoadConfiguation();
                    Configuration.ClearLoadedSettings();
                    Output.WriteLine("Resume");
                    Output.WriteLine("Configuration changes will be automatically saved now.");
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
                        else if (setitem == "LOG_WATCH_INTERVAL")
                        {
                            Configuration.LOG_WATCH_INTERVAL = int.Parse(args[2]);
                        }
                        else if (setitem == "MAX_LOG_SIZE")
                        {
                            Configuration.MAX_LOG_SIZE = int.Parse(args[2]);
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
                        else if (setitem == "EnableRange")
                        {
                            try
                            {
                                Configuration.EnableRange = bool.Parse(args[2]);

                            }
                            catch (Exception)
                            {
                                Output.SetForegroundColor(ConsoleColor.Red);
                                Output.WriteLine("Key \"EnableRange\" only accepts bool type(true and false)!");
                                Output.ResetColor();
                            }
                        }
                        else if (setitem == "LogUA")
                        {
                            try
                            {
                                Configuration.LogUA = bool.Parse(args[2]);

                            }
                            catch (Exception)
                            {
                                Output.SetForegroundColor(ConsoleColor.Red);
                                Output.WriteLine("Key \"LogUA\" only accepts bool type(true and false)!");
                                Output.ResetColor();
                            }
                        }
                    }
                    else
                    {
                        Output.SetForegroundColor(ConsoleColor.Red);
                        Output.WriteLine("Arguments does not match: Config set <key> <value>");
                        Output.ResetColor();
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
                        Output.WriteLine("arguments does not match: Config add <key> <value>");
                    }
                }
                else if (args[0].ToUpper() == "RM" || args[0].ToUpper() == "REMOVE")
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
                        Output.WriteLine("Arguments does not match: Config add <key> <value>");
                    }
                }
            }
            else
            {
                Output.WriteLine("Please specify an operation.");
            }
        }
    }
}

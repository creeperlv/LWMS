using System;
using System.Collections.Generic;
using System.Text;
using LWMS.Core;
using System.Diagnostics;
using LWMS.Localization;
using LWMS.Core.Configuration;

namespace LWMS.Management.Commands
{
    public class Config : IManageCommand
    {
        public string CommandName => "Config";

        public List<string> Alias => new List<string>();

        public int Version => 2;

        public static void OutputHelp()
        {
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Usage", "Usage:"));
            Output.WriteLine("");
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Usage", "\tConfig <Operation> [<Key> <Value>]"));
            Output.WriteLine("");
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Operations", "Operations:"));
            Output.WriteLine("");
            Output.WriteLine("\tRelease");
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Operations.Release", "\t\tStop writing changes to settings file, and settings file will be released in the same tim. It means settings file will be editable from other application."));
            Output.WriteLine("\tResume");
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Operations.Resume", "\t\tContinue writing changes to settings file, changes during the release will not be saved."));
            Output.WriteLine("\tReload");
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Operations.Reload", "\t\tReload settings."));
            Output.WriteLine("\tSet");
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Operations.Set", "\t\tSet a value to target key."));
            Output.WriteLine("\tAdd");
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Operations.Add", "\t\tAdd a value to target collection whose name is given Key."));
            Output.WriteLine("\tRemove,Rm");
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Operations.Remove", "\t\tRemove a value to target collection whose name is given Key."));
        }

        public void Invoke(params CommandPack[] args)
        {
            if (args.Length > 0)
            {
                string operation = args[0];
                if (operation.ToUpper() == "RELEASE")
                {
                    if (GlobalConfiguration.ConfigurationData != null)
                    {

                        GlobalConfiguration.ConfigurationData.Dispose();
                        GlobalConfiguration.ConfigurationData = null;
                        Output.WriteLine(Language.Query("ManageCmd.Config.Release.Tip0", "GlobalConfiguration file is released and changes will not be saved."));

                    }
                    else
                    {
                        Output.SetForegroundColor(ConsoleColor.Yellow);
                        Output.WriteLine(Language.Query("ManageCmd.Config.Release.Tip1", "GlobalConfiguration file is already released."));
                        Output.ResetColor();
                    }
                }
                else if (operation.ToUpper() == "RESUME")
                {
                    GlobalConfiguration.LoadConfiguation();
                    GlobalConfiguration.ClearLoadedSettings();
                    Output.WriteLine(Language.Query("ManageCmd.Config.Resume.Tip0", "Resumed."));
                    Output.WriteLine(Language.Query("ManageCmd.Config.Resume.Tip1", "GlobalConfiguration changes will be automatically saved now."));
                }
                else if (operation.ToUpper() == "RELOAD")
                {
                    GlobalConfiguration.LoadConfiguation();
                    GlobalConfiguration.ClearLoadedSettings();
                }
                else if (operation.ToUpper() == "SET")
                {
                    if (args.Length >= 3)
                    {
                        string setitem = args[1].ToUpper();
                        if (setitem == "BUF_LENGTH")
                        {
                            GlobalConfiguration.BUF_LENGTH = int.Parse(args[2]);
                        }
                        else if (setitem == "LOG_WATCH_INTERVAL")
                        {
                            GlobalConfiguration.LOG_WATCH_INTERVAL = int.Parse(args[2]);
                        }
                        else if (setitem == "MAX_LOG_SIZE")
                        {
                            GlobalConfiguration.MAX_LOG_SIZE = int.Parse(args[2]);
                        }
                        else if (setitem == "WEBROOT" || setitem == "WEBSITEROOT" || setitem == "WEBSITECONTENTROOT" || setitem == "WEBCONTENTROOT" || setitem == "CONTENTROOT")
                        {
                            GlobalConfiguration.WebSiteContentRoot = args[2];
                        }
                        else if (setitem == "DEFAULTPAGE")
                        {
                            GlobalConfiguration.DefaultPage = args[2];
                        }
                        else if (setitem == "404PAGE")
                        {
                            GlobalConfiguration.Page404 = args[2];
                        }
                        else if (setitem == "LANGUAGE")
                        {
                            var a=args[2].ToString().Replace("\"",null);
                            a=args[2].ToString().Replace("\'",null);
                            GlobalConfiguration.Language = args[2];
                            Output.SetForegroundColor(ConsoleColor.Yellow);
                            Output.WriteLine("Language is set to:"+GlobalConfiguration.Language);
                            Output.ResetColor();
                            Language.Initialize(args[2]);
                        }
                        else if (setitem == "ENABLERANGE")
                        {
                            try
                            {
                                GlobalConfiguration.EnableRange = bool.Parse(args[2]);

                            }
                            catch (Exception)
                            {
                                Output.SetForegroundColor(ConsoleColor.Red);
                                Output.WriteLine("Key \"EnableRange\" only accepts bool type(true and false)!");
                                Output.ResetColor();
                            }
                        }
                        else if (setitem == "LOGUA")
                        {
                            try
                            {
                                GlobalConfiguration.LogUA = bool.Parse(args[2]);

                            }
                            catch (Exception)
                            {
                                Output.SetForegroundColor(ConsoleColor.Red);
                                Output.WriteLine("Key \"LogUA\" only accepts bool type(true and false)!");
                                Output.ResetColor();
                            }
                        }
                        else
                        {
                            Output.WriteLine(Language.Query("ManageCmd.Config.UnidentifiedKey", "Unidentified Key: {0}" ,setitem));
                        }
                    }
                    else
                    {
                        Output.SetForegroundColor(ConsoleColor.Red);
                        Output.WriteLine(Language.Query("ManageCmd.Config.Set.ParameterMismatch", "Arguments does not match: Config set <key> <value>"));
                        Output.ResetColor();
                    }
                }
                else if (operation.ToUpper() == "ADD")
                {
                    if (args.Length >= 3)
                    {
                        string setitem = args[1].ToUpper();
                        if (setitem == "LISTENPREFIX" || setitem == "LISTEN")
                        {
                            var prefixes = GlobalConfiguration.ListenPrefixes;
                            prefixes.Add(args[2]);
                            GlobalConfiguration.ListenPrefixes = prefixes;
                        }
                    }
                    else
                    {
                        Output.SetForegroundColor(ConsoleColor.Red);
                        Output.WriteLine(Language.Query("ManageCmd.Config.Add.ParameterMismatch", "Arguments does not match: Config add <key> <value>"));
                        Output.ResetColor();
                    }
                }
                else if (operation.ToUpper() == "RM" || operation.ToUpper() == "REMOVE")
                {

                    if (args.Length >= 3)
                    {
                        string setitem = args[1].ToUpper();
                        if (setitem == "LISTENPREFIX" || setitem == "LISTEN")
                        {
                            try
                            {

                                var prefixes = GlobalConfiguration.ListenPrefixes;
                                prefixes.Remove(args[2]);
                                GlobalConfiguration.ListenPrefixes = prefixes;

                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    else
                    {
                        Output.WriteLine(Language.Query("ManageCmd.Config.Remove.ParameterMismatch", "Arguments does not match: Config remove <key>"));
                    }
                }else if(operation.ToUpper() == "H"|| operation.ToUpper() == "-H"|| operation.ToUpper() == "--H"|| operation.ToUpper() == "HELP"|| operation.ToUpper() == "?"|| operation.ToUpper() == "-?"|| operation.ToUpper() == "--?")
                {
                    OutputHelp();
                }
            }
            else
            {
                Output.SetForegroundColor(ConsoleColor.Yellow);
                Output.WriteLine(Language.Query("ManageCmd.Help.Config.Error.NoOperation","Please specify an operation."));
                Output.ResetColor();
                OutputHelp();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using LWMS.Core;
using System.Diagnostics;
using LWMS.Localization;
using LWMS.Core.Configuration;
using LWMS.Core.Authentication;

namespace LWMS.Management.Commands
{
    public class Config : IManageCommand
    {
        public string CommandName => "Config";

        public List<string> Alias => new List<string>();

        public int Version => 5;

        public static void OutputHelp(string AuthContext)
        {
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Usage", "Usage:"), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Usage", "\tConfig <Operation> [<Key> <Value>]"), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Operations", "Operations:"), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine("\tRelease", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Operations.Release", "\t\tStop writing changes to settings file, and settings file will be released in the same tim. It means settings file will be editable from other application."), AuthContext);
            Output.WriteLine("\tResume", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Operations.Resume", "\t\tContinue writing changes to settings file, changes during the release will not be saved."), AuthContext);
            Output.WriteLine("\tReload", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Operations.Reload", "\t\tReload settings."), AuthContext);
            Output.WriteLine("\tSet", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Operations.Set", "\t\tSet a value to target key."), AuthContext);
            Output.WriteLine("\tAdd", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Operations.Add", "\t\tAdd a value to target collection whose name is given Key."), AuthContext);
            Output.WriteLine("\tRemove,Rm", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Config.Operations.Remove", "\t\tRemove a value to target collection whose name is given Key."), AuthContext);
        }

        public void Invoke(string AuthContext, params CommandPack[] args)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () =>
            {
                if (args.Length > 0)
                {
                    string operation = args[0];
                    if (operation.ToUpper() == "RELEASE")
                    {
                        if (GlobalConfiguration.Release(AuthContext) == true)
                        {
                            Output.WriteLine(Language.Query("ManageCmd.Config.Release.Tip0", "GlobalConfiguration file is released and changes will not be saved."), AuthContext);

                        }
                        else
                        {
                            Output.SetForegroundColor(ConsoleColor.Yellow, AuthContext);
                            Output.WriteLine(Language.Query("ManageCmd.Config.Release.Tip1", "GlobalConfiguration file is already released."), AuthContext);
                            Output.ResetColor(AuthContext);
                        }
                    }
                    else if (operation.ToUpper() == "RESUME")
                    {
                        GlobalConfiguration.LoadConfiguation();
                        GlobalConfiguration.ClearLoadedSettings();
                        Output.WriteLine(Language.Query("ManageCmd.Config.Resume.Tip0", "Resumed."), AuthContext);
                        Output.WriteLine(Language.Query("ManageCmd.Config.Resume.Tip1", "GlobalConfiguration changes will be automatically saved now."), AuthContext);
                    }
                    else if (operation.ToUpper() == "RELOAD")
                    {
                        GlobalConfiguration.LoadConfiguation();
                        GlobalConfiguration.ClearLoadedSettings();
                    }else if (operation.ToUpper() == "LS")
                    {
                        var list = GlobalConfiguration.ListValues(AuthContext);
                        foreach (var item in list)
                        {
                            Output.WriteLine(item.Key + " : " + item.Value, AuthContext);
                        }
                    }
                    else if (operation.ToUpper() == "SET")
                    {
                        if (args.Length >= 3)
                        {
                            string setitem = args[1].ToUpper();
                            if (setitem == "BUF_LENGTH")
                            {
                                GlobalConfiguration.SetBUF_LENGTH(AuthContext,int.Parse(args[2]));
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
                                GlobalConfiguration.SetWebSiteContentRoot(AuthContext,args[2]);
                            }
                            else if (setitem == "DEFAULTPAGE")
                            {
                                GlobalConfiguration.SetDefaultPage(AuthContext, args[2]);
                            }
                            else if (setitem == "404PAGE")
                            {
                                GlobalConfiguration.SetPage404(AuthContext, args[2]);
                            }
                            else if (setitem == "LANGUAGE")
                            {
                                var a = args[2].ToString().Replace("\"", null);
                                a = args[2].ToString().Replace("\'", null);
                                GlobalConfiguration.Language = args[2];
                                Output.SetForegroundColor(ConsoleColor.Yellow, AuthContext);
                                Output.WriteLine("Language is set to:" + GlobalConfiguration.Language, AuthContext);
                                Output.ResetColor(AuthContext);
                                Language.Initialize(args[2]);
                            }
                            else if (setitem == "ENABLERANGE")
                            {
                                try
                                {
                                    GlobalConfiguration.SetEnableRange(AuthContext,bool.Parse(args[2]));

                                }
                                catch (Exception)
                                {
                                    Output.SetForegroundColor(ConsoleColor.Red, AuthContext);
                                    Output.WriteLine("Key \"EnableRange\" only accepts bool type(true and false)!", AuthContext);
                                    Output.ResetColor(AuthContext);
                                }
                            }
                            else if (setitem == "LOGUA")
                            {
                                try
                                {
                                    GlobalConfiguration.SetLogUA (AuthContext, bool.Parse(args[2]));

                                }
                                catch (Exception)
                                {
                                    Output.SetForegroundColor(ConsoleColor.Red, AuthContext);
                                    Output.WriteLine("Key \"LogUA\" only accepts bool type(true and false)!", AuthContext);
                                    Output.ResetColor(AuthContext);
                                }
                            }
                            else
                            {
                                GlobalConfiguration.SetValue(setitem, args[2],AuthContext);
//                                Output.WriteLine(Language.Query("ManageCmd.Config.UnidentifiedKey", "Unidentified Key: {0}", setitem), AuthContext);
                            }
                        }
                        else
                        {
                            Output.SetForegroundColor(ConsoleColor.Red, AuthContext);
                            Output.WriteLine(Language.Query("ManageCmd.Config.Set.ParameterMismatch", "Arguments does not match: Config set <key> <value>"), AuthContext);
                            Output.ResetColor(AuthContext);
                        }
                    }
                    else if (operation.ToUpper() == "ADD")
                    {
                        if (args.Length >= 3)
                        {
                            string setitem = args[1].ToUpper();
                            if (setitem == "LISTENPREFIX" || setitem == "LISTEN")
                            {
                                GlobalConfiguration.AddListenPrefix(AuthContext,args[2]);
                            }
                        }
                        else
                        {
                            Output.SetForegroundColor(ConsoleColor.Red, AuthContext);
                            Output.WriteLine(Language.Query("ManageCmd.Config.Add.ParameterMismatch", "Arguments does not match: Config add <key> <value>"), AuthContext);
                            Output.ResetColor(AuthContext);
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
                                    GlobalConfiguration.RemoveListenPrefix(AuthContext, args[2]);
                                }
                                catch (Exception)
                                {
                                }
                            }
                            else
                            {
                                GlobalConfiguration.DelValue(setitem, AuthContext);
                            }
                        }
                        else
                        {
                            Output.WriteLine(Language.Query("ManageCmd.Config.Remove.ParameterMismatch", "Arguments does not match: Config remove <key>"), AuthContext);
                        }
                    }
                    else if (operation.ToUpper() == "H" || operation.ToUpper() == "-H" || operation.ToUpper() == "--H" || operation.ToUpper() == "HELP" || operation.ToUpper() == "?" || operation.ToUpper() == "-?" || operation.ToUpper() == "--?")
                    {
                        OutputHelp(AuthContext);
                    }
                }
                else
                {
                    Output.SetForegroundColor(ConsoleColor.Yellow, AuthContext);
                    Output.WriteLine(Language.Query("ManageCmd.Help.Config.Error.NoOperation", "Please specify an operation."), AuthContext);
                    Output.ResetColor(AuthContext);
                    OutputHelp(AuthContext);
                }
            }, false, true, "Core.BasicConfig");

        }
    }
}

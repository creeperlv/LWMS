using LWMS.Core.Authentication;
using LWMS.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LWMS.Management.Commands
{
    public class Auth : IManageCommand
    {
        public string CommandName => "auth";

        public List<string> Alias => new();

        public int Version => 1;
        public void PrintHelp()
        {
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Usage", "Usage:"));
            Output.WriteLine("");
            Output.WriteLine(Language.Query("ManageCmd.Help.Auth.Usage", "Auth <Operation 0> [parameter 0], [parameter 1]...,<Operation 1> [parameter 0], [parameter 1]...,..."));
            Output.WriteLine("");
            Output.WriteLine(Language.Query("ManageCmd.Help.Auth.Warning", "\tThis command requires execute context has \"Core.SetPermission\" permission."));
            Output.WriteLine("");
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Operations", "Operations:"));
            Output.WriteLine("");
            Output.WriteLine("\tCreate");
            Output.WriteLine(Language.Query("ManageCmd.Help.Auth.Create", "\t\tCreate an auth with given permission. \"Class1Admin\" is tier 1 administrator except \"Core.SetPermission\" permission."));
            Output.WriteLine("");
            Output.WriteLine("\tRemove");
            Output.WriteLine(Language.Query("ManageCmd.Help.Auth.Remove", "\t\tRemove an auth."));
            Output.WriteLine("");
            Output.WriteLine("\tSet");
            Output.WriteLine(Language.Query("ManageCmd.Help.Auth.Set", "\t\tSet a permission to given auth."));
            Output.WriteLine("");
        }
        public void Invoke(string AuthContext, params CommandPack[] args)
        {
            if (args.Length == 0)
            {
                Output.SetForegroundColor(ConsoleColor.Yellow);
                Output.WriteLine(Language.Query("ManageCmd.Help.Config.Error.NoOperation", "Please specify an operation."));
                Output.ResetColor();
                PrintHelp();
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                try
                {

                    string Operation = args[i];
                    switch (Operation.ToUpper())
                    {
                        case "CREATE":
                            {
                                i++;
                                var UN = args[i].PackTotal;
                                i++;
                                var PW = args[i].PackTotal;
                                i++;
                                var PID = args[i].PackTotal;
                                i++;
                                var P = args[i].PackTotal;
                                bool P_ = bool.Parse(P);
                                OperatorAuthentication.SetPermission(AuthContext, OperatorAuthentication.ObtainAuth(UN, PW), PID, P_,UN);
                                Output.WriteLine(Language.Query("ManageCmd.Universal.Succeed", "Succeed."));
                            }
                            break;
                        case "REMOVE":
                        case "RM":
                            {
                                i++;
                                var Auth = args[i].PackTotal;
                                OperatorAuthentication.RemoveAuth(AuthContext, Auth);
                                Output.WriteLine(Language.Query("ManageCmd.Universal.Succeed", "Succeed."));
                            }
                            break;
                        case "SET":
                            {
                                i++;
                                var Auth = args[i].PackTotal;

                                i++;
                                var PID = args[i].PackTotal;
                                i++;
                                var P = args[i].PackTotal;
                                bool P_ = bool.Parse(P);
                                OperatorAuthentication.SetPermission(AuthContext, Auth, PID, P_);
                                Output.WriteLine(Language.Query("ManageCmd.Universal.Succeed", "Succeed."));
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Output.WriteLine(Language.Query("ManageCmd.Universal.Failed", "Failed."));
                    Output.WriteLine(e.Message);
                }
            }
        }
    }
}

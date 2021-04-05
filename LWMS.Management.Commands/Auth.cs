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

        public int Version => 2;
        public void PrintHelp(string AuthContext)
        {
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Usage", "Usage:"), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Auth.Usage", "Auth <Operation 0> [parameter 0], [parameter 1]...,<Operation 1> [parameter 0], [parameter 1]...,..."), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Auth.Warning", "\tThis command requires execute context has \"Core.SetPermission\" permission."), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Universal.Operations", "Operations:"), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine("\tCreate [Username] [Password]", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Auth.Create", "\t\tCreate an auth."), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine("\tRemove [AuthID]", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Auth.Remove", "\t\tRemove an auth."), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine("\tSet [AuthID] [PermissionID] [Value]", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Auth.Set", "\t\tSet a permission to given auth. \"Class1Admin\" is tier 1 administrator except \"Core.SetPermission\" permission."), AuthContext);
            Output.WriteLine("", AuthContext);
            Output.WriteLine("\tList|Ls", AuthContext);
            Output.WriteLine(Language.Query("ManageCmd.Help.Auth.List", "\t\tList all auth IDs with username."), AuthContext);
            Output.WriteLine("", AuthContext);
        }
        public void Invoke(string AuthContext, params CommandPack[] args)
        {
            if (args.Length == 0)
            {
                Output.SetForegroundColor(ConsoleColor.Yellow, AuthContext);
                Output.WriteLine(Language.Query("ManageCmd.Help.Config.Error.NoOperation", "Please specify an operation."), AuthContext);
                Output.ResetColor(AuthContext);
                PrintHelp(AuthContext);
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
                                OperatorAuthentication.CreateAuth(AuthContext,UN,PW);
                                Output.WriteLine(Language.Query("ManageCmd.Universal.Succeed", "Succeed."), AuthContext);
                            }
                            break;
                        case "REMOVE":
                        case "RM":
                            {
                                i++;
                                var Auth = args[i].PackTotal;
                                OperatorAuthentication.RemoveAuth(AuthContext, Auth);
                                Output.WriteLine(Language.Query("ManageCmd.Universal.Succeed", "Succeed."), AuthContext);
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
                                Output.WriteLine(Language.Query("ManageCmd.Universal.Succeed", "Succeed."), AuthContext);
                            }
                            break;
                        case "LIST":
                        case "LS":
                            {
                                var ls=OperatorAuthentication.ObtainAuthList(AuthContext);
                                foreach (var item in ls)
                                {
                                    Console.WriteLine(item.Value + ": " + item.Key);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Output.WriteLine(Language.Query("ManageCmd.Universal.Failed", "Failed."), AuthContext);
                    Output.WriteLine(e.Message, AuthContext);
                }
            }
        }
    }
}

using LWMS.Core.Authentication;
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

        }
        public void Invoke(string AuthContext, params CommandPack[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
            }

            for (int i = 0; i < args.Length; i++)
            {
                string Operation = args[i];
                switch (Operation.ToUpper())
                {
                    case "CREATE":
                        {
                            i++;
                            var UN = args[i];
                            i++;
                            var PW = args[i];
                            i++;
                            var PID = args[i];
                            i++;
                            var P = args[i];
                            bool P_ = bool.Parse(P);
                            OperatorAuthentication.SetPermission(AuthContext, OperatorAuthentication.ObtainAuth(UN, PW), PID, P_);
                        }
                        break;
                    case "REMOVE":
                    case "RM":
                        {

                            i++;
                            var Auth = args[i];
                            OperatorAuthentication.RemoveAuth(AuthContext, Auth);
                        }
                        break;
                    case "SET":
                        {
                            i++;
                            var Auth = args[i];

                            i++;
                            var PID = args[i];
                            i++;
                            var P = args[i];
                            bool P_ = bool.Parse(P);
                            OperatorAuthentication.SetPermission(AuthContext, Auth, PID, P_);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

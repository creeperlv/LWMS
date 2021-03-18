using LWMS.Core.Authentication;
using LWMS.Core.Configuration;
using LWMS.Core.SBSDomain;
using LWMS.Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LWMS.EventDrivenSupport
{
    public static class EDRPermissions
    {
        public const string RegisterHandler = "EDR.Handlers.RegisterHandler";
        public const string HandlerAll = "EDR.Handlers.All";
        public const string UnregisterHandler = "EDR.Handlers.UnregisterHandler";
        public const string UpdateDLL = "EDR.Handlers.UpdateDLL";
    }
    public class EDRMgr : IManageCommand
    {
        public string CommandName => "edrmgr";

        public List<string> Alias => new() { "edr", "edrmanager", "manage-edr" };

        public int Version => 1;

        public void Invoke(string AuthContext, params CommandPack[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var command = args[i];
                var cmd = command.PackTotal.ToUpper();
                switch (cmd)
                {
                    case "REGISTER":
                    case "ADDHANDLER":
                    case "--R":
                    case "--A":
                        {
                            var urlPrefix = args[i + 1].PackTotal;
                            var Assembly = args[i + 2].PackTotal;
                            var TargetType = args[i + 3].PackTotal;
                            i += 3;
                            OperatorAuthentication.AuthedAction(AuthContext, () =>
                            {
                                ApplicationConfiguration.Current.AddValueToArray("RoutedRequests", urlPrefix);
                                ApplicationConfiguration.Current.AddValueToArray("RouteTargets", Assembly + "," + TargetType);
                                {
                                    FileInfo fi = new(Assembly);
                                    var asm = DomainManager.LoadFromFile(AuthContext, fi.FullName);
                                    var t = asm.GetType(TargetType);
                                    MappedType mappedType = new MappedType(fi.Name, Activator.CreateInstance(t));
                                    EDREntry.RouteTargets.Add(mappedType);
                                    EDREntry.requests = null;
                                }
                            }, false, true, EDRPermissions.RegisterHandler, EDRPermissions.HandlerAll);
                        }
                        break;
                    case "UNREGISTER":
                    case "REMOVEHANDLER":
                    case "--UN":
                    case "--RM":
                        {
                            var urlPrefix = args[i + 1].PackTotal;
                            i++;
                            OperatorAuthentication.AuthedAction(AuthContext, () =>
                            {
                                var requests = ApplicationConfiguration.Current.GetValueArray("RoutedRequests");
                                if (requests is not null)
                                {
                                    for (int _i = 0; _i < requests.Length; _i++)
                                    {
                                        if (requests[_i].ToUpper() == urlPrefix.ToUpper())
                                        {
                                            ApplicationConfiguration.Current.RemoveValueFromArray("RoutedRequests", _i);
                                            ApplicationConfiguration.Current.RemoveValueFromArray("RouteTargets", _i);
                                            EDREntry.requests = null;
                                            EDREntry.RouteTargets = null;// Force reload.
                                            break;
                                        }
                                    }
                                }
                            }, false, false, EDRPermissions.UnregisterHandler, EDRPermissions.HandlerAll);
                        }
                        break;
                    case "UPDATEDLL":
                    case "--U":
                        {
                            OperatorAuthentication.AuthedAction(AuthContext, () =>
                            {
                                foreach (var item in EDREntry.RouteTargets)
                                {
                                    item.Update(AuthContext);
                                }
                            }, false, false, EDRPermissions.UpdateDLL, EDRPermissions.HandlerAll);
                        }
                        break;
                }
            }
        }
    }
}

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
    public class EDSMgr : IManageCommand
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
                            ApplicationConfiguration.Current.AddValueToArray("RoutedRequests", urlPrefix);
                            ApplicationConfiguration.Current.AddValueToArray("RouteTargets",Assembly+","+TargetType);
                            {
                                FileInfo fi = new(Assembly);
                                var asm = DomainManager.LoadFromFile(AuthContext, fi.FullName);
                                var t = asm.GetType(TargetType);
                                MappedType mappedType = new MappedType(fi.Name, Activator.CreateInstance(t));
                                EDSEntry.RouteTargets.Add(mappedType);
                            }
                            i += 3;
                        }
                        break;
                    case "UPDATEDLL":
                    case "--U":
                        {
                            foreach (var item in EDSEntry.RouteTargets)
                            {
                                item.Update(AuthContext);
                            }
                        }
                        break;
                }
            }
        }
    }
}

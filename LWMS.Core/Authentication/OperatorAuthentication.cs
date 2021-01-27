using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LWMS.Core.Authentication
{
    public static class OperatorAuthentication
    {
        private static string CurrentLocalHost = null;
        private static Dictionary<string, List<Permission>> Auths = new Dictionary<string, List<Permission>>();
        static OperatorAuthentication()
        {

        }
        public static void SetLocalHostAuth(string Auth)
        {
            if (CurrentLocalHost == null)
            {
                CurrentLocalHost = Auth;
            }
        }
        public static bool IsAuthed(string Auth, string PermissionID, bool DefaultPermission = false)
        {
            if (CurrentLocalHost == Auth) return true;
            if (!Auths.ContainsKey(Auth))
                return DefaultPermission;
            else
            {
                foreach (var item in Auths[Auth])
                {
                    if (item.ID == PermissionID)
                    {
                        return item.IsAllowed;
                    }
                }
            }
            return DefaultPermission;
        }
        public static string ObtainAuth(string Name, string Password)
        {
            var d = SHA256.HashData(Encoding.UTF8.GetBytes(Name + "," + Password));
            var s = Convert.ToBase64String(d);
            return s;
        }
    }
    internal class Permission
    {
        internal string ID;
        internal bool IsAllowed;
    }
}

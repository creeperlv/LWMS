using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using System;
using System.Collections.Generic;
using System.IO;
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
            LoadAuthentications();
        }
        static void LoadAuthentications()
        {
            DirectoryInfo di = new DirectoryInfo("./Auths/");
            if (!di.Exists) di.Create();
            foreach (var item in di.EnumerateFiles())
            {
                var auth = INILikeData.LoadFromWR(new FileWR(item));
                string name = auth.FindValue("Auth");
                Auths.Add(name, new());
                foreach (var permission in auth)
                {
                    if (permission.Key != "Auth")
                    {
                        Permission p = new Permission(permission.Key, bool.Parse(permission.Value));
                        Auths[name].Add(p);
                    }
                }
            }
        }
        public static int GetAuthCount() => Auths.Count;
        static void SaveAuth(string Auth)
        {
            string FN = Auth.Replace("/", "_").Replace("+", "_").Replace("=", "_").Replace("\\", "_");
            if (File.Exists(FN)) File.Delete(FN);
            File.Create(FN).Close();
            var f = INILikeData.CreateToFile(new FileInfo("./Auths/" + FN));
            f.AddValue("Auth", Auth, true, false, 0);
            foreach (var item in Auths[Auth])
            {
                f.AddValue(item.ID, item.IsAllowed + "", true, false, 0);
            }
            f.RemoveOldDuplicatedItems();
            f.Flush();
        }
        public static void SetLocalHostAuth(string Auth)
        {
            if (CurrentLocalHost == null)
            {
                CurrentLocalHost = Auth;
            }
        }
        public static void SetPermission(string ContextAuth, string OperateAuth, string PermissionID, bool Permission)
        {
            AuthedAction(ContextAuth, "Core.SetPermission", () =>
            {
                if (!Auths.ContainsKey(OperateAuth))
                {
                    Auths.Add(OperateAuth, new());
                }
                bool isOperated = false;
                foreach (var item in Auths[OperateAuth])
                {
                    if (item.ID == PermissionID)
                    {
                        item.IsAllowed = Permission;
                        isOperated = true;
                        break;
                    }
                }
                if (isOperated == false)
                {
                    Auths[OperateAuth].Add(new(PermissionID, Permission));
                }
                SaveAuth(OperateAuth);
            }, false);
        }
        public static void AuthedAction(string Auth, string PermissionID, Action action, bool DefaultPermission = false)
        {
            if (IsAuthed(Auth, PermissionID, DefaultPermission))
            {
                action();
            }
            else
            {
                throw new UnauthorizedException(Auth, PermissionID);
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
                    if (item.ID == "Class1Admin")
                    {
                        if (item.IsAllowed == true)
                        {
                            if (PermissionID != "Core.SetPermission")
                                return true;
                        }
                    }
                }
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
        public static bool IsAuthPresent(string Auth)
        {
            return Auth.Contains(Auth);
        }
        public static bool HasAdmin()
        {
            foreach (var item in Auths)
            {
                return IsAuthed(item.Key, "Class1Admin");
            }
            return false;
        }
        public static string ObtainAuth(string Name, string Password)
        {
            var d = SHA256.HashData(Encoding.UTF8.GetBytes(Name + "," + Password));
            var s = Convert.ToBase64String(d);
            return s;
        }
    }

    [Serializable]
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string Auth, string PermissionID) : base($"{Auth} is not allow to operate for \"{PermissionID}\" is not enabled.") { }
    }
    internal class Permission
    {
        readonly string id;
        public string ID { get => id; }
        internal bool IsAllowed;

        public Permission(string ID, bool isAllowed)
        {
            id = ID;
            IsAllowed = isAllowed;
        }
    }
}

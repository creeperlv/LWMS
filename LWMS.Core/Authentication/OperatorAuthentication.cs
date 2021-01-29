using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using LWMS.Localization;
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
        private static Dictionary<string, Authentication> Auths = new();
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
                string authID = auth.FindValue("Auth");
                string name = auth.FindValue("Name");
                Auths.Add(authID, new(authID, name));
                foreach (var permission in auth)
                {
                    if (permission.Key != "Auth")
                    {
                        Auths[authID].SetPermission(permission.Key, bool.Parse(permission.Value));
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
            f.AddValue("Name", Auths[Auth].Name, true, false, 0);
            foreach (var item in Auths[Auth].Permissions)
            {
                f.AddValue(item.Key, item.Value + "", true, false, 0);
            }
            f.RemoveOldDuplicatedItems();
            f.Flush();
        }
        public static void RemoveAuth(string ContextAuth, string TargetAuth)
        {
            AuthedAction(ContextAuth, "Core.SetPermission", () =>
            {
                if (Auths.ContainsKey(TargetAuth))
                    Auths.Remove(TargetAuth);
                string FN = TargetAuth.Replace("/", "_").Replace("+", "_").Replace("=", "_").Replace("\\", "_");
                if (File.Exists(FN)) File.Delete(FN);
            });
        }
        public static void SetLocalHostAuth(string Auth)
        {
            if (CurrentLocalHost == null)
            {
                CurrentLocalHost = Auth;
            }
        }
        public static string GetAuthName(string Name)
        {
            if (Auths.ContainsKey(Name))
            {
                return Auths[Name].Name;
            }
            return null;
        }
        public static void SetPermission(string ContextAuth, string OperateAuth, string PermissionID, bool Permission, string Name = null)
        {
            AuthedAction(ContextAuth, "Core.SetPermission", () =>
            {
                if (!Auths.ContainsKey(OperateAuth))
                {
                    Auths.Add(OperateAuth, new(OperateAuth, Name));
                }
                Auths[OperateAuth].SetPermission(PermissionID, Permission);
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
                if (Auths.ContainsKey(Auth))
                {
                    throw new UnauthorizedException(Auths[Auth].Name, PermissionID);

                }
                else
                    throw new UnauthorizedException(Auth, PermissionID);
            }
        }
        public static void AuthedAction(string Auth, Action action, bool DefaultPermission = false, bool ProduceError = false, params string[] PermissionIDs)
        {
            foreach (var item in PermissionIDs)
            {
                if (IsAuthed(Auth, item, DefaultPermission) is true)
                {
                    action();
                    return;
                }
            }
            if (ProduceError is true)
            {

                if (Auths.ContainsKey(Auth))
                {
                    throw new UnauthorizedException(Auths[Auth].Name, PermissionIDs[0]);

                }
                else
                    throw new UnauthorizedException(Auth, PermissionIDs[0]);
            }
            else return;
        }
        public static bool IsAuthed(string Auth, string PermissionID, bool DefaultPermission = false)
        {
            if (CurrentLocalHost == Auth) return true;
            if (!Auths.ContainsKey(Auth))
                return DefaultPermission;
            else
            {
                if (Auths[Auth].Permissions.ContainsKey("Class1Admin"))
                {
                    if (Auths[Auth].Permissions["Class1Admin"] is true)
                    {

                        if (PermissionID != "Core.SetPermission")
                            return true;
                    }
                }
                if (Auths[Auth].Permissions.ContainsKey(PermissionID))
                {
                    return Auths[Auth].Permissions[PermissionID];
                }
            }
            return DefaultPermission;
        }
        public static bool IsAuthPresent(string Auth)
        {
            return Auths.ContainsKey(Auth);
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
        public UnauthorizedException(string Auth, string PermissionID) : base(Language.Query("LWMS.Auth.Reject", "Operation rejected: auth {0} have no permission of {1}.", Auth, PermissionID)) { }
    }
    internal class Authentication
    {
        internal readonly string Name;
        internal readonly string AuthID;
        internal Dictionary<string, bool> Permissions = new Dictionary<string, bool>();
        public Authentication(string AuthID, string Name)
        {
            this.AuthID = AuthID;
            this.Name = Name;
        }
        internal void SetPermission(string ID, bool value)
        {
            if (Permissions.ContainsKey(ID))
            {
                Permissions[ID] = value;
            }
            else Permissions.Add(ID, value);
        }
    }
}

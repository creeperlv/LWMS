using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using CLUNL.Utilities;
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
    public static class PermissionID
    {
        public static readonly string SetPermission = "Core.SetPermission";
        public static readonly string ListAuths = "Auth.List";
        public static readonly string RegisterCmdModule= "Core.CommandModule.Register";
        public static readonly string UnregisterCmdModule= "Core.CommandModule.Unregister";
        public static readonly string CmdModuleAll= "Core.CommandModule.All";
    }
    public static class OperatorAuthentication
    {
        private static string CurrentLocalHost = null;
        private static string CurrentTrustedInstaller = null;
        private static Dictionary<string, Authentication> Auths = new();
        private static Dictionary<string, string> RuntimeAuth2AuthMap = new();
        private static string RuntimeSalt = null;
        static OperatorAuthentication()
        {
            RuntimeSalt = RandomTool.GetRandomString(24, RandomStringRange.R3);
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
                string pw = auth.FindValue("Password");
                Auths.Add(authID, new(authID, name, pw));
                RuntimeAuth2AuthMap.Add(ObtainRTAuth(name, pw), authID);
                foreach (var permission in auth)
                {
                    if (permission.Key != "Auth" && permission.Key != "Password" && permission.Key != "Name")
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
            if (File.Exists("./Auths/" + FN)) File.Delete("./Auths/" + FN);
            File.Create("./Auths/" + FN).Close();
            var f = INILikeData.CreateToFile(new FileInfo("./Auths/" + FN));
            f.AddValue("Auth", Auth, true, false, 0);
            f.AddValue("Name", Auths[Auth].Name, true, false, 0);
            f.AddValue("Password", Auths[Auth].Password, true, false, 0);
            foreach (var item in Auths[Auth].Permissions)
            {
                f.AddValue(item.Key, item.Value + "", true, false, 0);
            }
            f.RemoveOldDuplicatedItems();
            f.Flush();
            f.Dispose();
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
        public static void SetTrustedInstaller(string Auth)
        {
            if (CurrentTrustedInstaller == null)
            {
                CurrentTrustedInstaller = Auth;
            }
        }
        public static string CreateAuth(string Context, string Name, string Password)
        {
            if (IsAuthed(Context, "Core.SetPermission", false))
            {

                var AuthID = Guid.NewGuid().ToString();
                var RTAuth = ObtainRTAuth(Name, Password);
                RuntimeAuth2AuthMap.Add(RTAuth, AuthID);
                Auths.Add(AuthID, new Authentication(AuthID, Name, Password));
                SaveAuth(AuthID);
                return AuthID;

            }
            else
            {
                if (RuntimeAuth2AuthMap.ContainsKey(Context))
                {
                    throw new UnauthorizedException(RuntimeAuth2AuthMap[Context], "Core.SetPermission");
                }
                else
                {
                    throw new UnauthorizedException(Context, "Core.SetPermission");
                }
            }
        }
        public static string GetAuthName(string AuthID)
        {
            if (Auths.ContainsKey(AuthID))
            {
                return Auths[AuthID].Name;
            }
            return null;
        }
        public static void SetPermission(string ContextAuth, string TargetID, string PermissionID, bool Permission, string Name = null)
        {
            AuthedAction(ContextAuth, "Core.SetPermission", () =>
            {
                if (!Auths.ContainsKey(TargetID))
                {
                    throw new KeyNotFoundException();
                }
                Auths[TargetID].SetPermission(PermissionID, Permission);
                SaveAuth(TargetID);
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
            if (CurrentTrustedInstaller == Auth) return true;
            if (!RuntimeAuth2AuthMap.ContainsKey(Auth))
                return DefaultPermission;
            else
            {
                if (Auths[RuntimeAuth2AuthMap[Auth]].Permissions.ContainsKey("Class1Admin"))
                {
                    if (Auths[RuntimeAuth2AuthMap[Auth]].Permissions["Class1Admin"] is true)
                    {

                        if (PermissionID != "Core.SetPermission")
                            return true;
                    }
                }
                if (Auths[RuntimeAuth2AuthMap[Auth]].Permissions.ContainsKey(PermissionID))
                {
                    return Auths[RuntimeAuth2AuthMap[Auth]].Permissions[PermissionID];
                }
            }
            return DefaultPermission;
        }
        public static bool IsAuthPresent(string AuthID)
        {
            return RuntimeAuth2AuthMap.ContainsKey(AuthID);
        }
        public static Dictionary<string, string> ObtainAuthList(string Context)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            AuthedAction(Context, () =>
            {
                foreach (var item in Auths)
                {
                    dict.Add(item.Key, item.Value.Name);
                }
            }, false, true, PermissionID.ListAuths);
            return dict;
        }
        public static bool HasAdmin()
        {
            foreach (var item in Auths)
            {
                if(item.Value.GetPermission("Class1Admin")==true)
                return true;
            }
            return false;
        }
        public static string ObtainAuth(string Name, string Password)
        {
            //var d = SHA256.HashData(Encoding.UTF8.GetBytes(Name + "," + Password));
            //var s = Convert.ToBase64String(d);
            return ObtainRTAuth(Name, Password);
        }
        public static string ObtainRTAuth(string Name, string Password)
        {
            var d = SHA256.HashData(Encoding.UTF8.GetBytes(Name + "," + Password + "+" + RuntimeSalt));
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
        internal readonly string Password;
        internal readonly string AuthID;
        internal Dictionary<string, bool> Permissions = new Dictionary<string, bool>();
        public Authentication(string AuthID, string Name, string Password)
        {
            this.AuthID = AuthID;
            this.Name = Name;
            this.Password = Password;
        }
        internal bool GetPermission(string ID, bool DefaultValue = false)
        {
            if (Permissions.ContainsKey(ID))
            {
                return Permissions[ID];
            }
            return DefaultValue;
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

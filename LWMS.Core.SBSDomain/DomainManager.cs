using LWMS.Core.Authentication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LWMS.Core.SBSDomain
{
    public class DomainManager
    {
        internal static string TrustedInstaller = null;
        public static void SetTrustedInstaller(string Auth)
        {
            if (TrustedInstaller is null)
            {
                TrustedInstaller = Auth;
            }
        }
        static Dictionary<string, List<Assembly>> DLLs = new Dictionary<string, List<Assembly>>();
        public static Assembly LoadFromFile(string Auth, string PathToDLL)
        {
            Assembly v = null;
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                var guid = Guid.NewGuid();
                var temp = Path.Combine(Path.GetTempPath(), guid.ToString());
                FileInfo fi = new(PathToDLL);
                Directory.CreateDirectory(temp);
                var Final = fi.CopyTo(Path.Combine(temp + fi.Name));
                v = Assembly.LoadFrom(Final.FullName);
                if (!DLLs.ContainsKey(fi.Name))
                {
                    DLLs.Add(fi.Name, new());
                }
                DLLs[fi.Name].Add(v);
            }, false, true, PermissionID.Core_SBS_Load, PermissionID.Core_SBS_All);
            return v;
        }
        public static void UpdateMappedTypes(string AuthContext, string LibFileName)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () =>
            {
                foreach (var item in MappedType.MappedTypeObjectCollection)
                {
                    if (item.LibFileName == LibFileName)
                    {
                        item.Update(AuthContext);
                    }
                }

            }, false, true, PermissionID.Core_SBS_Update, PermissionID.Core_SBS_All);
        }
        public static Assembly GetAssembly(string Auth, string Name, int Version = 0)
        {
            Assembly v = null;
            OperatorAuthentication.AuthedAction(Auth, () =>
            {
                v = DLLs[Name][Version];
            }, false, true, PermissionID.Core_SBS_Read, PermissionID.Core_SBS_All);
            return v;
        }
    }
    public class MappedType : IDisposable
    {
        internal static List<MappedType> MappedTypeObjectCollection = new();
        internal List<object> InitParameters = new List<object>();
        public static MappedType CreateFrom(Type t, params object[] parameters)
        {
            FileInfo fi = new FileInfo(
            t.Assembly.Location);
            MappedType mappedType = new MappedType(fi.Name, Activator.CreateInstance(t, parameters));
            if (parameters is not null)
            {
                if (parameters.Length > 0)
                {
                    mappedType.InitParameters = new List<object>(parameters);
                }
            }
            return mappedType;
        }
        public static MappedType CreateFrom(object obj)
        {
            FileInfo fi = new FileInfo(
            (obj.GetType()).Assembly.Location);
            MappedType mappedType = new MappedType(fi.Name, obj);
            return mappedType;
        }
        public static void UpdateAll(string AuthContext)
        {
            foreach (var item in MappedTypeObjectCollection)
            {
                item.Update(AuthContext);
            }
        }
        internal MappedType(string File, object _object)
        {
            LibFileName = File;
            TargetObject = _object;
            MappedTypeObjectCollection.Add(this);
        }
        public string LibFileName;
        public object TargetObject;
        internal void Update()
        {
            var asm = DomainManager.GetAssembly(DomainManager.TrustedInstaller, LibFileName);
            var t = asm.GetType(TargetObject.GetType().FullName);
            TargetObject = (object)Activator.CreateInstance(t,InitParameters.ToArray());
        }
        public void Update(string AuthContext)
        {
            var asm = DomainManager.GetAssembly(AuthContext, LibFileName);
            var t = asm.GetType(TargetObject.GetType().FullName);
            TargetObject = (object)Activator.CreateInstance(t, InitParameters.ToArray());

        }

        public void Dispose()
        {
            if (MappedTypeObjectCollection.Contains(this))
                MappedTypeObjectCollection.Remove(this);
            if (TargetObject is IDisposable) (TargetObject as IDisposable).Dispose();
            TargetObject = null;

        }
    }
}

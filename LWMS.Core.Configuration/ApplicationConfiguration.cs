﻿using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using LWMS.Core.FileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LWMS.Core.Configuration
{
    public class ApplicationConfiguration
    {
        public static ApplicationConfiguration Current
        {
            get
            {
                StackTrace st = new StackTrace(1);
                var item = st.GetFrame(0);
                var ModuleName = item.GetMethod().DeclaringType.Assembly.GetName().Name;
                ApplicationConfiguration configuration;
                if (keyValuePairs.ContainsKey(ModuleName))
                {
                    configuration = keyValuePairs[ModuleName];
                }
                else
                {

                    configuration = new ApplicationConfiguration(ModuleName);
                    keyValuePairs.Add(ModuleName, configuration);
                }

                return configuration;
            }
        }
        internal static Dictionary<string, ApplicationConfiguration> keyValuePairs = new Dictionary<string, ApplicationConfiguration>();
        StorageFile sf;
        internal ApplicationConfiguration(string ModuleName)
        {
            var moduleConfigFolder = ApplicationStorage.Configuration.CreateFolder(GlobalConfiguration.TrustedInstaller, "Modules", true);
            bool b = moduleConfigFolder.CreateFile(GlobalConfiguration.TrustedInstaller, ModuleName + ".ini", out sf);

            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(sf.Parent.ItemPath, sf.Name);
            fileSystemWatcher.Changed += (_a, _b) =>
            {
                RawData = INILikeData.LoadFromWR(new FileWR(sf.ToFileInfo(GlobalConfiguration.TrustedInstaller)));
            };
            //var fs = moduleConfigFolder.GetFiles(GlobalConfiguration.TrustedInstaller);
            //foreach (var item in fs)
            //{
            //    if (item.Name == ModuleName + ".ini")
            //    {
            //        RawData = INILikeData.LoadFromWR(new FileWR(item.ToFileInfo(GlobalConfiguration.TrustedInstaller)));
            //        return;
            //    }
            //}
            if (b == false)
                RawData = INILikeData.LoadFromWR(new FileWR(sf.ToFileInfo(GlobalConfiguration.TrustedInstaller)));
            else RawData = INILikeData.CreateToWR(new FileWR(sf.ToFileInfo(GlobalConfiguration.TrustedInstaller)));


        }
        public void Reload()
        {
            RawData = INILikeData.LoadFromWR(new FileWR(sf.ToFileInfo(GlobalConfiguration.TrustedInstaller)));
        }
        INILikeData RawData;
        public string GetValue(string Key, string fallback = null)
        {
            string v = null;
            v = RawData.FindValue(Key);
            if (v is null) v = fallback;
            return v;
        }
        public void SetValue(string Key, string Value)
        {
            RawData.AddValue(Key, Value, false, true);
        }
        public void Clear()
        {
            RawData.Clear();
        }
        public List<KeyValuePair<string, string>> ListValues()
        {
            List<KeyValuePair<string, string>> rs = new();

            {
                if (RawData != null)
                {
                    foreach (var item in RawData)
                    {
                        rs.Add(new(item.Key, item.Value));
                    }
                }
            }
            return rs;
        }
        public string[] GetValueArray(string Key)
        {
            string[] v = null;
            var _count = RawData.FindValue(Key + ".Count");
            if (_count is not null)
            {
                try
                {
                    int count = int.Parse(_count);
                    v = new string[count];
                    for (int i = 0; i < count; i++)
                    {
                        v[i] = RawData.FindValue(Key + "." + i);
                    }
                }
                catch (Exception)
                {
                }
            }
            return v;
        }
        public void AddValueToArray(string Key, string value)
        {
            var c = RawData.FindValue(Key + ".Count");
            if (c == null)
            {
                SetValueArray(Key, value);
            }
            else
            {
                var C = int.Parse(c);
                RawData.AddValue(Key + ".Count", (C + 1).ToString(), false, false);
                RawData.AddValue(Key + "." + C, value, false, false);
                RawData.Flush();
            }
            //RawData.AddValue(Key + ".Count", values.Length + "", true, false, Handle);
        }
        public void RemoveValueFromArray(string Key, int Index)
        {

            var c = RawData.FindValue(Key + ".Count");
            if (c == null)
            {
            }
            else
            {
                var C = int.Parse(c);
                var arr = new List<string>(GetValueArray(Key));
                arr.RemoveAt(Index);
                for (int i = Index; i < C; i++)
                {
                    RawData.DeleteKey(Key + "." + i, false);
                }
                RawData.Flush();
                SetValueArray(Key, arr.ToArray());
            }

        }
        public void SetValueArray(string Key, params string[] values)
        {
            Random r = new Random();
            int Handle = r.Next(0, int.MaxValue);
            RawData.OnHold(Handle);
            {

                var c = RawData.FindValue(Key + ".Count");
                if (c != null)
                {
                    //Remove old array.
                    int Count;
                    if (int.TryParse(c, out Count))
                    {

                        for (int i = 0; i < Count; i++)
                        {
                            RawData.DeleteKey(Key + "." + i, false, Handle);
                        }
                    }
                }
            }
            RawData.AddValue(Key + ".Count", values.Length + "", false, false, Handle);
            for (int i = 0; i < values.Length; i++)
            {
                RawData.AddValue(Key + "." + i, values[i], false, false, Handle);
            }
            RawData.Flush(Handle);
            RawData.Release(Handle);
        }
    }
}

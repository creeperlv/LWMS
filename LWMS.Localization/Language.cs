using CLUNL.Data.Layer1;
using CLUNL.DirectedIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace LWMS.Localization
{
    public static class Language
    {
        static Language()
        {

        }
        static Dictionary<string, string> LanguageStrings = new Dictionary<string, string>();
        /// <summary>
        /// Init language dictionary with given culture information string (e.g.: en-US).
        /// </summary>
        /// <param name="CultureInfo"></param>
        public static void Initialize(string CultureInfo)
        {
            Assembly assembly = typeof(Language).Assembly;
            DirectoryInfo location = new DirectoryInfo(Path.Combine(new FileInfo(assembly.Location).DirectoryName, "Locales", CultureInfo.ToUpper()));
            DirectoryInfo location_en = new DirectoryInfo(Path.Combine(new FileInfo(assembly.Location).DirectoryName, "Locales", "EN-US"));
            if (!location.Exists)
            {
                location = new DirectoryInfo(Path.Combine(new FileInfo(assembly.Location).DirectoryName, "Locales", "EN-US"));
            }
            if (!location.Exists)
            {
                Trace.WriteLine("Cannot find applicable language files.");
                return;
            }
            LanguageStrings.Clear();
            //Load english first.
            foreach (var item in location_en.EnumerateFiles())
            {
                INILikeData keyValuePairs = INILikeData.LoadFromWR(new FileWR(item));
                string Namespace = keyValuePairs.FindValue("Namespace");
                foreach (var ID in keyValuePairs)
                {
                    if (ID.Key != "Namespace")
                    {
                        var Key = $"{Namespace}.{ID.Key}";
                        if (LanguageStrings.ContainsKey(Key)) LanguageStrings[Key] = ID.Value;
                        else LanguageStrings.Add(Key, ID.Value);
                    }
                }
                keyValuePairs.Dispose();
            }

            //Then load target language.
            if (location.FullName != location_en.FullName)
                foreach (var item in location.EnumerateFiles())
                {
                    INILikeData keyValuePairs = INILikeData.LoadFromWR(new FileWR(item));
                    string Namespace = keyValuePairs.FindValue("Namespace");
                    foreach (var ID in keyValuePairs)
                    {
                        if (ID.Key != "Namespace")
                        {
                            var Key = $"{Namespace}.{ID.Key}";
                            if (LanguageStrings.ContainsKey(Key)) LanguageStrings[Key] = ID.Value;
                            else LanguageStrings.Add(Key, ID.Value);
                        }
                    }
                    keyValuePairs.Dispose();
                }

        }
        public static string Query(string ID, string Fallback = "Translation not exist!", params string[] Parameters)
        {
            if (LanguageStrings.ContainsKey(ID))
            {
                var value = LanguageStrings[ID].Replace("\\n", Environment.NewLine).Replace("\\t", "\t");
                return string.Format(value, Parameters);

            }
            else
            {
                var value = Fallback.Replace("\\n", Environment.NewLine).Replace("\\t", "\t");
                return string.Format(value, Parameters);
            }
        }
    }
}

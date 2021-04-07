using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace LWMS.RemoteShell.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Copyright (C) 2020 Creeper Lv");
            Console.WriteLine("This software is licensed under the MIT License");
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, "LWMS.RemoveShell.ConsoleClient");
            var Conf = Path.Combine(path, "Settings.ini");
            SimpleConfig config = null;
            if (File.Exists(Conf))
            {
                config = SimpleConfig.LoadFromFile(Conf);
            }
            IPEndPoint endPoint;
            string Address=null;
            int port=-1;
            if(config is not null)
            {
                Address = config.GetValue("ServerAddress", null);
                {
                    int.TryParse(config.GetValue("ServerPort", null), out port);
                }
            }
        }
    }
    class SimpleConfig
    {
        Dictionary<string, string> CoreData = new();
        internal string GetValue(string key, string fallback = "")
        {
            if (CoreData.ContainsKey(key))
            {
                return CoreData[key];
            }
            else return fallback;
        }
        internal static SimpleConfig LoadFromFile(string FileName)
        {
            SimpleConfig simpleConfig = new SimpleConfig();
            var lines = File.ReadAllLines(FileName);
            foreach (var item in lines)
            {
                var d = item.Trim();
                if (d.StartsWith("#"))
                {
                    //Ignore
                }
                else
                {
                    if (d.IndexOf("=") > 0)
                    {
                        var k = d.Substring(0, d.IndexOf('='));
                        var v = d.Substring(d.IndexOf('=') + 1);
                        if (simpleConfig.CoreData.ContainsKey(k))
                            simpleConfig.CoreData[k] = v;
                        else simpleConfig.CoreData.Add(k, v);
                    }
                    else
                    {
                        Console.WriteLine("W: Line:" + item + " in configuration file is not understandable.");
                        //Ignore
                    }
                }
            }
            return simpleConfig;
        }
    }

}

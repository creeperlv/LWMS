using LWMS.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LWMS.Management.Commands
{
    public class Log : IManageCommand
    {
        public string CommandName => "ManageLog";

        List<string> alias = new List<string>();
        public int Version => 1;
        public Log()
        {
            alias.Add("log");
        }
        public List<string> Alias => alias;

        public void Invoke(params CommandPack[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0].PackTotal.ToUpper())
                {
                    case "LS":
                    case "LIST":
                        {
                            foreach (var item in Directory.EnumerateFiles(LWMSTraceListener.LogDir))
                            {
                                Trace.WriteLine(item);
                            }
                        }
                        break;
                    case "CLEAR":
                        {
                            try
                            {
                                foreach (var item in Directory.EnumerateFiles(LWMSTraceListener.LogDir))
                                {
                                    File.Delete(item);
                                    Trace.WriteLine("Log>>Delete:" + item);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

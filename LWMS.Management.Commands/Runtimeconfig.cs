using LWMS.Core;
using LWMS.Core.Authentication;
using LWMS.Core.Configuration;
using LWMS.Core.Log;
using LWMS.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LWMS.Management.Commands
{
    public class Runtimeconfig : IManageCommand
    {
        public string CommandName => "RuntimeConfig";
        List<string> alias = new List<string>();
        public Runtimeconfig()
        {
            alias.Add("rtcfg");
        }
        public List<string> Alias => alias;

        public int Version => 3;

        public void Invoke(string AuthContext, params CommandPack[] args)
        {
            OperatorAuthentication.AuthedAction(AuthContext, () => {
                for (int i = 0; i < args.Length; i++)
                {
                    var item = args[i];
                    switch (item.ToUpper())
                    {
                        case "/DISABLEBEAUTIFYCONSOLE":
                            LWMSTraceListener.SetProperty(AuthContext, 0, false);
                            //LWMSTraceListener.BeautifyConsoleOutput = false;
                            break;
                        case "/ENABLEBEAUTIFYCONSOLE":
                            LWMSTraceListener.SetProperty(AuthContext, 0, true);
                            //LWMSTraceListener.BeautifyConsoleOutput = true;
                            break;
                        case "/DISABLECONSOLE":
                            LWMSTraceListener.SetProperty(AuthContext, 1, false);
                            //LWMSTraceListener.EnableConsoleOutput = false;
                            break;
                        case "/DISABLELOGTOFILE":
                            LWMSTraceListener.SetProperty(AuthContext, 2, false);
                            //LWMSTraceListener.WriteToFile = false;
                            break;
                        case "/ENABLECONSOLE":
                            LWMSTraceListener.SetProperty(AuthContext, 1, true);
                            //LWMSTraceListener.EnableConsoleOutput = true;
                            break;
                        default:
                            {
                                switch (item.PackParted[0].ToUpper())
                                {
                                    case "BUF_LENGTH":
                                        {
                                            int B;
                                            int.TryParse(args[i].PackParted[1], out B);
                                            GlobalConfiguration.Set_BUF_LENGTH_RT(AuthContext,B);
                                            Output.WriteLine(Language.Query("ManageCmd.RuntimeConfig.SetValue", "{0} is temporarily set to {1} without saving to GlobalConfiguration file.", "BUT_LENGTH", B.ToString()), AuthContext);
                                        }
                                        break;
                                    case "WEBROOT":
                                        {
                                            string path = args[i].PackParted[1];
                                            GlobalConfiguration.Set_WebRoot_RT(AuthContext, path);
                                            Output.WriteLine(Language.Query("ManageCmd.RuntimeConfig.SetValue", "{0} is temporarily set to {1} without saving to GlobalConfiguration file.", "WebRoot", path), AuthContext);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                    }
                }
            }, false, true, "Core.Config.Runtime");
        }
    }
}

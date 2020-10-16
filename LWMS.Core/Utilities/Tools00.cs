using LWMS.Core.HttpRoutedLayer;
using LWMS.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LWMS.Core.Utilities
{
    public class Tools00
    {
        /// <summary>
        /// Send a file to specific http listener context with given status code.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="f"></param>
        /// <param name="StatusCode"></param>
        public static void SendFile(HttpListenerRoutedContext context, FileInfo f, int StatusCode = 200)
        {
            using (FileStream fs = f.OpenRead())
            {
                try
                {
                    Trace.WriteLine("Access:" + f.FullName.Substring(Configuration.WebSiteContentRoot.Length));
                    
                    var BUF_LENGTH = Configuration.BUF_LENGTH;
                    byte[] buf = new byte[BUF_LENGTH];
                    if (f.Extension == ".html")
                    {
                        context.Response.ContentType = "text/html";
                    }
                    context.Response.ContentLength64 = f.Length;
                    context.Response.StatusCode = StatusCode;
                    context.Response.ContentEncoding = Encoding.UTF8;
                    int L = 0;
                    while ((L = fs.Read(buf, 0, BUF_LENGTH)) != 0)
                    {
                        context.Response.OutputStream.Write(buf, 0, L);
                        context.Response.OutputStream.Flush();
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Cannot send file:" + e);
                }
            }
        }
        /// <summary>
        /// Send a message to specific http listener context with given status code.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="Message"></param>
        /// <param name="StatusCode"></param>
        public static void SendMessage(HttpListenerRoutedContext context, string Message, int StatusCode = 200)
        {
            var bytes = Encoding.UTF8.GetBytes(Message);
            context.Response.ContentType = "text/html";
            context.Response.ContentLength64 = bytes.Length;
            context.Response.StatusCode = StatusCode;
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.OutputStream.Write(bytes);
            context.Response.OutputStream.Flush();
        }
        public static List<Match> CommandParse(string cmd)
        {
            //https://regexlib.com/REDetails.aspx?regexp_id=13053
            //var m=Regex.Match(cmd, " *(?:-+([^= \\\'\\\"]+)[= ]?)?(?:([\\\'\\\"])([^2]+?)\\2|([^- \"\']+))?");
            var m=Regex.Match(cmd, " *(?:-+([^=: \\\'\\\"]+)[= ]?)?(?:([\\\'\\\"])([^2]+?)\\2|([^- \"\']+))?");
            List<Match> cmdList=new List<Match>();
            while (m.Success)
            {
                cmdList.Add(m);
                m = m.NextMatch();
            }
            return cmdList;
        }
        public static List<CommandPack> ResolveCommand(string cmd)
        {
            List<CommandPack> cmdps = new List<CommandPack>(); 
            foreach (var item in CommandParse(cmd))
            {
                var cmdp = CommandPack.FromRegexMatch(item);
                if(cmdp.PackTotal!="")
                cmdps.Add(cmdp);
            }
            return cmdps;
        }
    }
}

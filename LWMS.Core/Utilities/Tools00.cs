using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LWMS.Core.Utilities
{
    public class Tools00
    {
        public static void SendFile(HttpListenerContext c, FileInfo f, int StatusCode = 200)
        {
            //using(FileStream fs=f.OpenRead())
            using (FileStream fs = f.OpenRead())
            {
                try
                {
                    Trace.WriteLine("Access:" + f.FullName.Substring(Configuration.WebSiteContentRoot.Length));

                    var BUF_LENGTH = Configuration.BUF_LENGTH;
                    byte[] buf = new byte[BUF_LENGTH];
                    if (f.Extension == ".html")
                    {
                        c.Response.ContentType = "text/html";
                    }
                    c.Response.ContentLength64 = f.Length;
                    c.Response.StatusCode = StatusCode;
                    c.Response.ContentEncoding = Encoding.UTF8;
                    int L = 0;
                    while ((L = fs.Read(buf, 0, BUF_LENGTH)) != 0)
                    {
                        c.Response.OutputStream.Write(buf, 0, L);
                        c.Response.OutputStream.Flush();
                    }
                }
                catch (Exception e)
                {

                }
            }
        }
        public static void SendMessage(HttpListenerContext c, string Message, int StatusCode = 200)
        {
            var bytes = Encoding.UTF8.GetBytes(Message);
            c.Response.ContentType = "text/html";
            c.Response.ContentLength64 = bytes.Length;
            c.Response.StatusCode = StatusCode;
            c.Response.ContentEncoding = Encoding.UTF8;
            c.Response.OutputStream.Write(bytes);
            c.Response.OutputStream.Flush();
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
    }
}

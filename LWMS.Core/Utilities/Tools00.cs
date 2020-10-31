using LWMS.Core.HttpRoutedLayer;
using LWMS.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LWMS.Core.Utilities
{
    public struct Range
    {
        public static Range Empty = new Range() { L = long.MinValue, R = long.MinValue };
        public long L;
        public long R;
        public override string ToString()
        {
            if (L == long.MinValue)
            {
                if (R == long.MinValue)
                    return $"-";
                return $"-{R}";
            }
            else if (R == long.MinValue)
                return $"{L}-";
            else return $"{L}-{R}";
        }
        public override bool Equals(object obj)
        {
            if (obj is Range)
            {
                var item = (Range)obj;
                return item.R == R && item.L == L;
            }
            else
                return base.Equals(obj);
        }
        public static Range FromString(string str)
        {
            str = str.Trim();
            var g = str.Split('-');
            Range range = new Range();
            if (g[0] == null || g[0] == "") range.L = long.MinValue; else range.L = long.Parse(g[0]);
            if (g[1] == null || g[1] == "") range.R = long.MinValue; else range.R = long.Parse(g[0]);
            return range;
        }
    }
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
                    context.Response.AddHeader("Accept-Ranges", "true");
                    string range = null;
                    List<Range> ranges = new List<Range>();
                    try
                    {
                        range = context.Request.Headers["Range"];
                        range = range.Trim();
                        range = range.Substring(6);
                        var rs = range.Split(',');
                        foreach (var item in rs)
                        {
                            ranges.Add(Range.FromString(item));
                        }
                    }
                    catch (Exception)
                    {
                    }
                    if (ranges.Count == 0)
                    {

                        int L = 0;
                        while ((L = fs.Read(buf, 0, BUF_LENGTH)) != 0)
                        {
                            context.Response.OutputStream.Write(buf, 0, L);
                            context.Response.OutputStream.Flush();
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 206;
                        context.Response.StatusDescription = "Partial Content";
                        foreach (var item in ranges)
                        {
                            long length = 0;
                            long L = 0;
                            if (item.R == long.MinValue)
                            {
                                length = fs.Length - item.L;
                            }else if (item.L == long.MinValue)
                            {
                                length = item.R;
                            }
                            else
                            {
                                length = item.R - item.L;
                            }
                            if (item.L != long.MinValue)
                            {
                                L = item.L;
                            }
                            fs.Position = L;
                            byte[] b=null;
                            while (L<length)
                            {
                                if (length - L > BUF_LENGTH)
                                {
                                    L+=fs.Read(b, 0, BUF_LENGTH);
                                }
                                else
                                {
                                    L += fs.Read(b, 0,(int)( length - L));
                                }
                                context.Response.OutputStream.Write(b, 0, b.Length);
                                context.Response.OutputStream.Flush();
                            }
                        }
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
            var m = Regex.Match(cmd, " *(?:-+([^=: \\\'\\\"]+)[= ]?)?(?:([\\\'\\\"])([^2]+?)\\2|([^- \"\']+))?");
            List<Match> cmdList = new List<Match>();
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
                if (cmdp.PackTotal != "")
                    cmdps.Add(cmdp);
            }
            return cmdps;
        }
    }
}

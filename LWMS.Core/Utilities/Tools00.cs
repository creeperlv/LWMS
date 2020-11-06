﻿using CLUNL;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Management;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace LWMS.Core.Utilities
{
    public class Tools00
    {
        public static string Boundary = "sierra117";
        /// <summary>
        /// Send a file to specific http listener context with given status code.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="f"></param>
        /// <param name="StatusCode"></param>
        public static void SendFile(HttpListenerRoutedContext context, FileInfo f, HttpStatusCode StatusCode = HttpStatusCode.OK)
        {
            using (FileStream fs = f.OpenRead())
            {
                try
                {
                    Trace.WriteLine("Access:" + f.FullName.Substring(Configuration.WebSiteContentRoot.Length));
                    var BUF_LENGTH = Configuration.BUF_LENGTH;
                    byte[] buf = new byte[BUF_LENGTH];
                    context.Response.ContentType = ObtainMimeType(f.Extension);
                    context.Response.ContentEncoding = Encoding.UTF8;
                    if (Configuration.EnableRange == true)
                        context.Response.AddHeader("Accept-Ranges", "bytes");
                    else
                        context.Response.AddHeader("Accept-Ranges", "none");
                    context.Response.Headers.Remove(HttpResponseHeader.Server);
                    context.Response.Headers.Set(HttpResponseHeader.Server, "LWMS/" + LWMSCoreServer.ServerVersion);
                    string range = null;
                    List<Range> ranges = new List<Range>();
                    if (context.Request.HttpMethod == HttpMethod.Head.Method)
                    {
                        context.Response.OutputStream.Flush();
                        return;
                    }
                    if (Configuration.EnableRange == true)
                        try
                        {
                            range = context.Request.Headers["Range"];
                            if (range != null)
                            {

                                range = range.Trim();
                                range = range.Substring(6);
                                var rs = range.Split(',');
                                foreach (var item in rs)
                                {
                                    ranges.Add(Range.FromString(item));
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    if (ranges.Count == 0)
                    {
                        context.Response.ContentLength64 = f.Length;
                        context.Response.StatusCode = (int)StatusCode;
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
                        var OriginalContentType = "Content-Type: " + context.Response.ContentType;
                        context.Response.ContentType = "multipart/byteranges; boundary=" + Boundary;
                        string _Boundary = "--" + Boundary;
                        var NewLine = Environment.NewLine;
                        foreach (var item in ranges)
                        {
                            //string header = _Boundary+"\r\n"+OriginalContentType + "\r\n" + "Content-Range: bytes " + item.ToString() + "/" + fs.Length+"\r\n\r\n" ;
                            context.Response.Headers.Add(HttpResponseHeader.ContentRange, "bytes " + item.ToString() + "/" + fs.Length);
                            long length = 0;
                            long L = 0;
                            {
                                //Calculate length to send and left-starting index.
                                if (item.R == long.MinValue || item.R > fs.Length)
                                {
                                    length = fs.Length - item.L;
                                }
                                else if (item.L == long.MinValue || item.L < 0)
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
                            }
                            fs.Seek(L, SeekOrigin.Begin);
                            //var b = Encoding.ASCII.GetBytes(header);
                            //context.Response.OutputStream.Write(b, 0, b.Length);
                            //context.Response.OutputStream.Flush();
                            int _Length;
                            while (L < length)
                            {
                                if (length - L > BUF_LENGTH)
                                {
                                    L += (_Length = fs.Read(buf, 0, BUF_LENGTH));
                                }
                                else
                                {
                                    L += (_Length = fs.Read(buf, 0, (int)(length - L)));
                                }
                                context.Response.OutputStream.Write(buf, 0, _Length);
                                context.Response.OutputStream.Flush();
                            }
                            //{
                            //    var v = Encoding.UTF8.GetBytes("\r\n");
                            //    context.Response.OutputStream.Write(v, 0, v.Length);

                            //}
                            break;
                        }
                        {
                            //var v = Encoding.UTF8.GetBytes("\r\n");
                            //context.Response.OutputStream.Write(v, 0, v.Length);

                        }
                        context.Response.OutputStream.Flush();
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Cannot send file:" + e.HResult);
                }
            }
        }
        /// <summary>
        /// Send a message to specific http listener context with given status code.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="Message"></param>
        /// <param name="StatusCode"></param>
        public static void SendMessage(HttpListenerRoutedContext context, string Message, HttpStatusCode StatusCode = HttpStatusCode.OK)
        {
            var bytes = Encoding.UTF8.GetBytes(Message);
            context.Response.ContentType = "text/html";
            context.Response.ContentLength64 = bytes.Length;
            context.Response.StatusCode = (int)StatusCode;
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.OutputStream.Write(bytes);
            context.Response.OutputStream.Flush();
        }
        static Dictionary<string, string> types = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
            { ".css", "text/css" },
            { ".c", "text/plain" },
            { ".cxx", "text/plain" },
            { ".cpp", "text/plain" },
            { ".cs", "text/plain" },
            { ".h", "text/plain" },
            { ".html", "text/html" },
            { ".rtf", "text/rtf" },
            { ".xml", "text/xml" } ,
            { ".htm", "text/html" },
            { ".js", "application/javascript" },
            { ".zip", "application/zip" },
            { ".png", "image/png" },
            { ".tiff","image/tiff"},
            { ".heif","image/heic"},
            { ".jpg","video/JPEG"},
            { ".bmp","image/bmp"},
            { ".ttf","font/ttf"},
            { ".woff","font/woff"},
            { ".woff2","font/woff2"},
            { ".otf","font/otf"},
        };
        /// <summary>
        /// Find a mime type by file extension name. (Including "." in the start of the string)
        /// </summary>
        /// <param name="FileExtension"></param>
        /// <returns></returns>
        public static string ObtainMimeType(string FileExtension)
        {
            if (types.ContainsKey(FileExtension))
            {
                return types[FileExtension];
            }
            else return "application/binary";
        }
        /// <summary>
        /// Analyze a command line into a Match list.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Analyze a command line into a CommandPack list.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
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

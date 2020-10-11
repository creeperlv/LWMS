using CLUNL.DirectedIO;
using CLUNL.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace LWMS.Core
{
    public class HttpPipelineProcessor : DefaultProcessor
    {

    }
    public class DefaultStaticFileUnit : IPipedProcessUnit
    {
        public static int BUF_LENGTH = 40960;
        public DefaultStaticFileUnit()
        {
            Console.WriteLine("Inited.");
        }
        public PipelineData Process(PipelineData Input)
        {

            HttpListenerContext context = Input.PrimaryData as HttpListenerContext;
            Console.WriteLine(context.Request.Url.LocalPath);
            var path0 = context.Request.Url.LocalPath.Substring(1);
            var path1 = Path.Combine(Configuration.WebSiteContentRoot, path0);
            //Console.WriteLine("Try:"+path1);
            if (Directory.Exists(path1))
            {
                var DefaultPage = Path.Combine(path1, Configuration.DefaultPage);
                //Console.WriteLine("Try:" + DefaultPage);
                if (File.Exists(DefaultPage))
                {
                    SendFile(Input.PrimaryData as HttpListenerContext, new FileInfo(DefaultPage));
                    //return Input;
                }
                else
                {

                }

            }
            else
            {

                Console.WriteLine("Try:" + path1);
                if (File.Exists(path1))
                {
                    SendFile(context, new FileInfo(path1));
                }
                else
                {
                    Console.WriteLine("Try:" + path1);

                }
                //Console.WriteLine("Directory Not Found.");
            }
            return Input;
        }
        public void SendFile(HttpListenerContext c, FileInfo f)
        {

            Console.WriteLine("Send:" + f.FullName);
            //using(FileStream fs=f.OpenRead())
            using (FileStream fs = f.OpenRead())
            {
                //try
                //{
                byte[] buf = new byte[BUF_LENGTH];
                Console.WriteLine(f.Extension);
                if (f.Extension == ".html")
                {
                    c.Response.ContentType = "text/html";
                }
                c.Response.StatusCode = 200;
                c.Response.ContentEncoding = Encoding.UTF8;
                c.Response.ContentLength64 = f.Length;
                int L = 0;
                while ((L=fs.Read(buf, 0, BUF_LENGTH)) != 0)
                {
                    c.Response.OutputStream.Write(buf, 0, L);
                c.Response.OutputStream.Flush();
                    //Console.WriteLine(Encoding.UTF8.GetString(buf));
                }
                //}
                //catch (Exception e)
                //{

                //}
            }
            Console.WriteLine("Transmited.");
        }
    }
}

using CLUNL;
using CLUNL.DirectedIO;
using CLUNL.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace LWMS.Core
{
    public class HttpPipelineProcessor : IPipelineProcessor
    {
        List<IPipedProcessUnit> processUnits = new List<IPipedProcessUnit>();
        public void Init(params IPipedProcessUnit[] units)
        {
            if (units.Length == 0)
            {
                processUnits.Add(new DefaultProcessUnit());
            }
            else
            {
                processUnits = new List<IPipedProcessUnit>(units);
            }
        }
        public void Init()
        {
            Init(new IPipedProcessUnit[0]);
        }

        public void Init(ProcessUnitManifest manifest)
        {
            processUnits = manifest.GetUnitInstances();
        }
        public PipelineData Process(PipelineData Input)
        {
            bool willIgnore = false;
            try
            {
                if (LibraryInfo.GetFlag(FeatureFlags.Pipeline_IgnoreError) == 1)
                {
                    willIgnore = true;
                }
            }
            catch (Exception)
            {
                //Ignore
            }
            return Process(Input, willIgnore);
        }

        public PipelineData Process(PipelineData Input, bool IgnoreError)
        {
            if ((Input.SecondaryData as HttpPipelineArguments).isHandled == true) return Input;
            if (IgnoreError)
            {

                foreach (var item in processUnits)
                {
                    try
                    {
                        var output = item.Process(Input);
                        if (Input.CheckContinuity(output))
                        {
                            Input = output;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                return Input;
            }
            else
            {

                foreach (var item in processUnits)
                {
                    var output = item.Process(Input);
                    if (Input.CheckContinuity(output))
                    {
                        Input = output;
                    }
                    else
                    {
                        throw new PipelineDataContinuityException(item);
                    }
                }
                return Input;
            }
        }
    }
    public class HttpPipelineArguments
    {
        public bool isHandled = false;
    }
    public class DefaultStaticFileUnit : IPipedProcessUnit
    {
        public static int BUF_LENGTH = 1048576;
        public DefaultStaticFileUnit()
        {
        }
        public PipelineData Process(PipelineData Input)
        {
            HttpListenerContext context = Input.PrimaryData as HttpListenerContext;
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
                    (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                }
                else
                {

                }

            }
            else
            {

                if (File.Exists(path1))
                {
                    SendFile(context, new FileInfo(path1));
                    (Input.SecondaryData as HttpPipelineArguments).isHandled = true;
                }
                else
                {
                }
                //Console.WriteLine("Directory Not Found.");
            }
            return Input;
        }
        public void SendFile(HttpListenerContext c, FileInfo f)
        {
            //using(FileStream fs=f.OpenRead())
            using (FileStream fs = f.OpenRead())
            {
                //try
                //{
                byte[] buf = new byte[BUF_LENGTH];
                if (f.Extension == ".html")
                {
                    c.Response.ContentType = "text/html";
                }
                c.Response.StatusCode = 200;
                c.Response.ContentEncoding = Encoding.UTF8;
                c.Response.ContentLength64 = f.Length;
                int L = 0;
                while ((L = fs.Read(buf, 0, BUF_LENGTH)) != 0)
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
        }
    }
}

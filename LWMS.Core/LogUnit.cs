using CLUNL.Pipeline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace LWMS.Core
{
    public class LogUnit : IPipedProcessUnit
    {
        public LogUnit()
        {
            Trace.Listeners.Add(new LWMSTraceListener());
        }
        public PipelineData Process(PipelineData Input)
        {
            StringBuilder b = new StringBuilder();
            var c = Input.PrimaryData as HttpListenerContext;
            b.Append(c.Request.RemoteEndPoint);
            b.Append(">>");
            b.Append(c.Request.HttpMethod);
            b.Append(">>");
            b.Append(c.Request.RawUrl);
            Trace.WriteLine(b);
            return Input;
        }
    }
    public class LWMSTraceListener : TraceListener
    {
        string LogFile;
        public static bool BeautifyConsoleOutput = false;
        public static bool EnableConsoleOutput = true;
        public static bool WriteToFile = true;
        public static string LogDir { get; internal set; }
        public LWMSTraceListener()
        {
            var LogBasePath = Path.Combine(Configuration.BasePath, "Logs");
            LogDir = LogBasePath;
            if (!Directory.Exists(LogBasePath))
            {
                Directory.CreateDirectory(LogBasePath);
            }
            var Now = DateTime.Now;
            LogFile = Path.Combine(LogBasePath, $"{Now.Year}-{Now.Month}-{Now.Day}-{Now.Minute}-{Now.Second}-{Now.Millisecond}.log");
            File.Create(LogFile).Close();
            File.WriteAllText(LogFile, "");
        }
        public override void Write(string message)
        {
            StackTrace stackTrace = new StackTrace(4);
            StringBuilder stringBuilder = new StringBuilder();
            var now = DateTime.Now;
            stringBuilder.Append("[");
            stringBuilder.Append(now);
            stringBuilder.Append("]");
            stringBuilder.Append("[");
            stringBuilder.Append(stackTrace.GetFrame(0).GetMethod().ReflectedType.FullName);
            stringBuilder.Append("]");
            stringBuilder.Append(message);
            if (EnableConsoleOutput == true)
            {
                if (BeautifyConsoleOutput == false)
                    Console.Write(stringBuilder);
                else
                {
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(now);
                    Console.ResetColor();
                    Console.Write("]");
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(stackTrace.GetFrame(0).GetMethod().ReflectedType.FullName); 
                    Console.ResetColor();
                    Console.Write("]");
                    Console.Write(message);
                }
            }
            if(WriteToFile)
            File.AppendAllText(LogFile, stringBuilder.ToString());
        }

        public override void WriteLine(string message)
        {
            StackTrace stackTrace = new StackTrace(4);
            StringBuilder stringBuilder = new StringBuilder();
            var now = DateTime.Now;
            stringBuilder.Append("[");
            stringBuilder.Append(now);
            stringBuilder.Append("]");
            stringBuilder.Append("[");
            stringBuilder.Append(stackTrace.GetFrame(0).GetMethod().ReflectedType.FullName);
            stringBuilder.Append("]");
            stringBuilder.Append(message);
            stringBuilder.Append(Environment.NewLine);
            if (EnableConsoleOutput == true)
            {
                if (BeautifyConsoleOutput == false)
                    Console.Write(stringBuilder);
                else
                {
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(now);
                    Console.ResetColor();
                    Console.Write("]");
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(stackTrace.GetFrame(0).GetMethod().ReflectedType.FullName);
                    Console.ResetColor();
                    Console.Write("]");
                    Console.Write(message);
                    Console.Write(Environment.NewLine);
                }
            }
            if (WriteToFile)
                File.AppendAllText(LogFile, stringBuilder.ToString());
        }
    }
}

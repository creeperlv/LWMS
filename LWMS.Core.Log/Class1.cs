using CLUNL.DirectedIO;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LWMS.Core.Log
{
    public class LWMSTraceListener : TraceListener
    {
        public static string CurrentLogFile;
        public static bool BeautifyConsoleOutput = false;
        public static bool EnableConsoleOutput = true;
        public static bool WriteToFile = true;
        public static string LogDir { get; internal set; }
        /// <summary>
        /// Do not operate this, it will be operated by LogWatcher task.
        /// </summary>
        internal static IBaseWR LogFile;
        public static ConcurrentQueue<string> ContentToLog = new ConcurrentQueue<string>();
        public static Task LogTask = null;
        static int OperatingID = 0;
        public LWMSTraceListener(string BasePath)
        {
            var LogBasePath = Path.Combine(BasePath, "Logs");
            LogDir = LogBasePath;
            if (!Directory.Exists(LogBasePath))
            {
                Directory.CreateDirectory(LogBasePath);
            }
            var Now = DateTime.Now;
            CurrentLogFile = Path.Combine(LogBasePath, $"{Now.Year}-{Now.Month}-{Now.Day}-{Now.Minute}-{Now.Second}-{Now.Millisecond}.log");
            File.Create(CurrentLogFile).Close();
            File.WriteAllText(CurrentLogFile, "");
            LogFile = new FileWR(new FileInfo(CurrentLogFile));
            Random random = new Random();
            OperatingID = random.Next();
            LogTask = Task.Run(LogWatcher);
        }
        public static void NewLogFile()
        {
            if (!Directory.Exists(LogDir))
            {
                Directory.CreateDirectory(LogDir);
            }
            Random random = new Random();
            OperatingID = random.Next();//ID Varied, task should exit immediately.
            if (LogTask != null) LogTask.Wait();
            var Now = DateTime.Now;
            CurrentLogFile = Path.Combine(LogDir, $"{Now.Year}-{Now.Month}-{Now.Day}-{Now.Minute}-{Now.Second}-{Now.Millisecond}.log");
            File.Create(CurrentLogFile).Close();
            File.WriteAllText(CurrentLogFile, "");
            LogFile = new FileWR(new FileInfo(CurrentLogFile));
            LogTask = Task.Run(LogWatcher);
        }
        static void LogWatcher()
        {
            int TID = OperatingID;
            while (TID == OperatingID)
            {
                if (WriteToFile == true)
                    if (ContentToLog.IsEmpty == false)
                    {
                        string content;
                        if (ContentToLog.TryDequeue(out content))
                        {
                            LogFile.Write(content);
                            LogFile.Flush();
                        }
                    }
            }
            LogFile.Dispose();
        }
        public static void WriteFile(string message)
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
            if (WriteToFile) ContentToLog.Enqueue(stringBuilder.ToString());
        }
        public static void WriteFileLine(string message)
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
            if (WriteToFile) ContentToLog.Enqueue(stringBuilder.ToString());
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
            if (WriteToFile) ContentToLog.Enqueue(stringBuilder.ToString());
            //LogFile.WriteLine(stringBuilder.ToString());
            //File.AppendAllText(CurrentLogFile, stringBuilder.ToString());
        }

        public override void WriteLine(string message)
        {
            StackTrace stackTrace = new StackTrace(true);
            StringBuilder stringBuilder = new StringBuilder();
            var t = stackTrace.GetFrame(3).GetMethod().ReflectedType;
            var now = DateTime.Now;
            stringBuilder.Append("[");
            stringBuilder.Append(now);
            stringBuilder.Append("]");
            stringBuilder.Append("[");
            if (t != typeof(Trace))
                stringBuilder.Append(t.FullName);
            else
                stringBuilder.Append(stackTrace.GetFrame(4).GetMethod().ReflectedType.FullName);
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
                    if (t != typeof(Trace))
                        Console.Write(t.FullName);
                    else
                        Console.Write(stackTrace.GetFrame(4).GetMethod().ReflectedType.FullName);
                    Console.ResetColor();
                    Console.Write("]");
                    Console.Write(message);
                    Console.Write(Environment.NewLine);
                }
            }
            if (WriteToFile) ContentToLog.Enqueue(stringBuilder.ToString());
            //if (WriteToFile)
            //    if (WriteToFile) LogFile.WriteLine(stringBuilder.ToString());
        }
    }
}

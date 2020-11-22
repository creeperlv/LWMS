using CLUNL.DirectedIO;
using LWMS.Core.WR;
using LWMS.Core.FileSystem;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;
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
        static int RemainContents = 0;
        static int OperatingID = 0;
        public static int _LOG_WATCH_INTERVAL = 5;
        public static int _MAX_LOG_SIZE = 0;
        /// <summary>
        /// Directly stop watching.
        /// </summary>
        public static void StopWatch()
        {
            OperatingID = -1;
        }
        public LWMSTraceListener(string BasePath)
        {
            
            LogDir = ApplicationStorage.Logs.ItemPath;
            var Now = DateTime.Now;
            StorageFile storageFile;
            ApplicationStorage.Logs.CreateFile($"{Now.Year}-{Now.Month}-{Now.Day}-{Now.Minute}-{Now.Second}-{Now.Millisecond}.log", out storageFile);
            CurrentLogFile = storageFile.ItemPath;
            LogFile = new FileWR(storageFile);
            Random random = new Random();
            OperatingID = random.Next();
            thread = new Thread(new ThreadStart(delegate () { LogWatcher(); }));
            thread.Priority = ThreadPriority.Lowest;
            thread.Start();
            //LogTask = Task.Run(LogWatcher);
        }
        static Thread thread;
        /// <summary>
        /// Create a new file to write logs.
        /// </summary>
        public static void NewLogFile()
        {
            Random random = new Random();
            OperatingID = random.Next();//ID Varied, task should exit immediately.
            //if (LogTask != null) thread.;
            var Now = DateTime.Now;
            StorageFile storageFile;
            ApplicationStorage.Logs.CreateFile($"{Now.Year}-{Now.Month}-{Now.Day}-{Now.Minute}-{Now.Second}-{Now.Millisecond}.log", out storageFile);
            CurrentLogFile = storageFile.ItemPath;
            LogFile = new FileWR(storageFile);
            thread = new Thread(new ThreadStart(delegate () { LogWatcher(); }));
            thread.Priority = ThreadPriority.Lowest;
            thread.Start();
            //LogTask = Task.Run(LogWatcher);
        }
        /// <summary>
        /// LogWatcher, will watch ContentToLog and write them to UsingWR.
        /// </summary>
        static void LogWatcher()
        {
            IBaseWR UsingWR = LogFile;
            int TID = OperatingID;
            while (TID == OperatingID)
            {
                if (WriteToFile == true)
                {

                    if (RemainContents != 0)
                    {

                        if (ContentToLog.IsEmpty == false)
                        {
                            string content;
                            if (ContentToLog.TryDequeue(out content))
                            {

                                UsingWR.Write(content);
                                UsingWR.Flush();

                                RemainContents--;
                                if (_MAX_LOG_SIZE != -1)
                                    if (UsingWR.Length >= _MAX_LOG_SIZE)
                                    {
                                        NewLogFile();
                                    }
                            }
                        }
                    }
                    else
                        Thread.Sleep(_LOG_WATCH_INTERVAL);
                }
                else
                {
                    Thread.Sleep(_LOG_WATCH_INTERVAL);
                }
            }
            UsingWR.Dispose();
        }
        /// <summary>
        /// Force to write remaining ContentToLog to UsingWR.
        /// </summary>
        public static void FlushImmediately()
        {
            lock (ContentToLog)
            {
                lock (LogFile)
                {
                    while (ContentToLog.IsEmpty!=true)
                    {
                        string content;
                        if (ContentToLog.TryDequeue(out content))
                        {

                            LogFile.Write(content);
                            LogFile.Flush();

                            RemainContents--;
                            if (_MAX_LOG_SIZE != -1)
                                if (LogFile.Length >= _MAX_LOG_SIZE)
                                {
                                    NewLogFile();
                                }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Add a message to ContentToLog but not showing to Console.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteFile(string message)
        {
            StackTrace stackTrace = new StackTrace(4);
            StringBuilder stringBuilder = new StringBuilder();
            Type t = stackTrace.GetFrame(0).GetMethod().ReflectedType;
            if (t == typeof(PipedRoutedConsoleLikeWR))
            {
                t = stackTrace.GetFrame(2).GetMethod().ReflectedType;
            }
            var now = DateTime.Now;
            stringBuilder.Append("[");
            stringBuilder.Append(now);
            stringBuilder.Append("]");
            stringBuilder.Append("[");
            stringBuilder.Append(t.FullName);
            stringBuilder.Append("]");
            stringBuilder.Append(message);
            if (WriteToFile)
            {
                ContentToLog.Enqueue(stringBuilder.ToString());
                RemainContents++;
            }
        }
        /// <summary>
        /// Add a message to ContentToLog with a new line but not showing to Console.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteFileLine(string message)
        {
            StackTrace stackTrace = new StackTrace(4);
            StringBuilder stringBuilder = new StringBuilder();
            Type t = stackTrace.GetFrame(0).GetMethod().ReflectedType;
            if (t == typeof(PipedRoutedConsoleLikeWR))
            {
                t = stackTrace.GetFrame(2).GetMethod().ReflectedType;
            }
            var now = DateTime.Now;
            stringBuilder.Append("[");
            stringBuilder.Append(now);
            stringBuilder.Append("]");
            stringBuilder.Append("[");
            stringBuilder.Append(t.FullName);
            stringBuilder.Append("]");
            stringBuilder.Append(message);
            stringBuilder.Append(Environment.NewLine);
            if (WriteToFile)
            {
                ContentToLog.Enqueue(stringBuilder.ToString());
                RemainContents++;
            }
        }
        /// <summary>
        /// Add a message to ContentToLog and write to Console.
        /// </summary>
        /// <param name="message"></param>
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
            if (WriteToFile)
            {
                ContentToLog.Enqueue(stringBuilder.ToString());
                RemainContents++;
            }
        }

        /// <summary>
        /// Add a message to ContentToLog with a new line and write to Console.
        /// </summary>
        /// <param name="message"></param>
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
            if (WriteToFile)
            {
                ContentToLog.Enqueue(stringBuilder.ToString());
                RemainContents++;
            }
        }
    }
}

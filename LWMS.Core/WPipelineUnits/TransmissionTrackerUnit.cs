using CLUNL.DirectedIO;
using CLUNL.Pipeline;
using LWMS.Core.HttpRoutedLayer;
using LWMS.Core.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace LWMS.Core.WPipelineUnits
{
    /// <summary>
    /// This unit can log almost all transmissions. This is needed to be added into W Pipeline.
    /// </summary>
    public class TransmissionTrackerUnit : IPipedProcessUnit
    {
        public PipelineData Process(PipelineData Input)
        {
            var Args = Input.Options as RoutedPipelineStreamOptions;
            var Data = Input.SecondaryData;
            switch (Args.Method)
            {
                case RoutedPipelineStreamMethod.READ:
                    SideLoger.CurrentLoger.Log($"R->{Args.Context.Request.RemoteEndPoint}<<{(Data as ReadArguments).Result}");
                    break;
                case RoutedPipelineStreamMethod.SEEK:
                    break;
                case RoutedPipelineStreamMethod.SETLENGTH:
                    break;
                case RoutedPipelineStreamMethod.WRITE:
                    SideLoger.CurrentLoger.Log($"W->{Args.Context.Request.RemoteEndPoint}>>{(Data as WriteArguments).Count}");
                    break;
                case RoutedPipelineStreamMethod.FLUSH:
                    SideLoger.CurrentLoger.Log($"F->{Args.Context.Request.RemoteEndPoint}");
                    break;
                default:
                    break;
            }
            return Input;
        }
    }
    /// <summary>
    /// A side logger along with TWMSTraceListener.
    /// </summary>
    public class SideLoger
    {
        public static SideLoger CurrentLoger = new SideLoger();
        string file;
        FileWR LogFileWR;
        int id = 0;
        SideLoger()
        {
            file = LWMSTraceListener.CurrentLogFile + "." + id + ".trace";
            LogFileWR = new FileWR(new System.IO.FileInfo(file));
        }
        public void Log(string str)
        {
            LogFileWR.WriteLine($"[{DateTime.Now}]{str}");
            if (Configuration.MAX_LOG_SIZE != -1)
                if (LogFileWR.Length >= Configuration.MAX_LOG_SIZE)
                {
                    NewLog();
                }
        }
        void NewLog()
        {
            LogFileWR.Dispose();
            var f = LWMSTraceListener.CurrentLogFile + "." + id + ".trace";
            if (file == f)
            {
                id++;
                file = LWMSTraceListener.CurrentLogFile + "." + id + ".trace";
            }
            else
            {
                id = 0;
                file = LWMSTraceListener.CurrentLogFile + "." + id + ".trace";
            }
            LogFileWR = new FileWR(new System.IO.FileInfo(file));
        }
    }
}

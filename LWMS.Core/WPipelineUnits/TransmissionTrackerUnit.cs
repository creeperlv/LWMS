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
    /// This unit can log almost all transmissions.
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
    public class SideLoger
    {
        public static SideLoger CurrentLoger = new SideLoger();
        string file;
        FileWR LogFileWR;
        SideLoger()
        {
            file=LWMSTraceListener.CurrentLogFile+".trace";
            LogFileWR = new FileWR(new System.IO.FileInfo(file));
        }
        public void Log(string str)
        {
            LogFileWR.WriteLine($"[{DateTime.Now}]{str}");
        }
    }
}

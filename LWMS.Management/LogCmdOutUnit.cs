using System;
using System.Collections.Generic;
using System.Text;
using CLUNL.Pipeline;
using LWMS.Core.Log;

namespace LWMS.Management
{
    public class LogCmdOutUnit : IPipedProcessUnit
    {
        public PipelineData Process(PipelineData Input)
        {
            PipedRoutedWROption option = (PipedRoutedWROption)Input.Options;
            if(option.PipedRoutedWROperation== PipedRoutedWROperation.WRITE)
            {
                LWMSTraceListener.WriteFile((string)Input.PrimaryData);
            }else if(option.PipedRoutedWROperation== PipedRoutedWROperation.WRITELINE)
            {
                LWMSTraceListener.WriteFileLine((string)Input.PrimaryData);
            }
            return Input;
        }
    }
}

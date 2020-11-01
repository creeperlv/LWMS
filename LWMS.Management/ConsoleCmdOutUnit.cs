using System;
using System.Collections.Generic;
using System.Text;
using CLUNL.Pipeline;
using LWMS.Core.WR;

namespace LWMS.Management
{
    public class ConsoleCmdOutUnit : IPipedProcessUnit
    {
        public PipelineData Process(PipelineData Input)
        {
            PipedRoutedWROption option = (PipedRoutedWROption)Input.Options;
            if(option.PipedRoutedWROperation== PipedRoutedWROperation.WRITE)
            {
                Console.Out.Write((string)Input.PrimaryData);
            }else if(option.PipedRoutedWROperation== PipedRoutedWROperation.WRITELINE)
            {
                Console.Out.WriteLine((string)Input.PrimaryData);
            }else if(option.PipedRoutedWROperation== PipedRoutedWROperation.FLUSH)
            {
                Console.Out.Flush();
            }
            return Input;
        }
    }
}

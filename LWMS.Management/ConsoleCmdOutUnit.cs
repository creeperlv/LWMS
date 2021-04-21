using System;
using System.Collections.Generic;
using System.Text;
using CLUNL.Pipeline;
using LWMS.Core.Log;
using LWMS.Core.WR;

namespace LWMS.Management
{
    /// <summary>
    /// A default CmdOut Pipelien Unit that write messages to console.
    /// </summary>
    public class ConsoleCmdOutUnit : IPipedProcessUnit
    {
        public PipelineData Process(PipelineData Input)
        {
            if (LWMSTraceListener.EnableConsoleOutput == true)
            {
                PipedRoutedWROption option = (PipedRoutedWROption)Input.Options;
                if (option.PipedRoutedWROperation == PipedRoutedWROperation.WRITE)
                {
                    Console.Out.Write((string)Input.PrimaryData);
                }
                else if (option.PipedRoutedWROperation == PipedRoutedWROperation.WRITELINE)
                {
                    Console.Out.WriteLine((string)Input.PrimaryData);
                }
                else if (option.PipedRoutedWROperation == PipedRoutedWROperation.FLUSH)
                {
                    Console.Out.Flush();
                }else if(option.PipedRoutedWROperation== PipedRoutedWROperation.CLEAR)
                {
                    Console.Clear();
                }
                else if (LWMSTraceListener.BeautifyConsoleOutput == true)
                {
                    if (option.PipedRoutedWROperation == PipedRoutedWROperation.FGCOLOR)
                    {
                        Console.ForegroundColor = (ConsoleColor)Input.PrimaryData;
                    }
                    else if (option.PipedRoutedWROperation == PipedRoutedWROperation.BGCOLOR)
                    {
                        Console.BackgroundColor = (ConsoleColor)Input.PrimaryData;
                    }
                    else if (option.PipedRoutedWROperation == PipedRoutedWROperation.RESETCOLOR)
                    {
                        Console.ResetColor();
                    }
                }
            }
            return Input;
        }
    }
}

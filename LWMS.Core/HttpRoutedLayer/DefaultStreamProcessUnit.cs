using CLUNL.Pipeline;
using System.Diagnostics;
using System.IO;

namespace LWMS.Core.HttpRoutedLayer
{
    public class DefaultStreamProcessUnit : IPipedProcessUnit
    {
        public PipelineData Process(PipelineData Input)
        {
            RoutedPipelineStreamOptions Options = (RoutedPipelineStreamOptions)(Input.Options);
            Stream BaseStream = (Stream)Input.PrimaryData;
            switch (Options.Method)
            {
                case RoutedPipelineStreamMethod.READ:
                    {
                        ReadArguments readArguments = (ReadArguments)Input.SecondaryData;
                        readArguments.Result = BaseStream.Read(readArguments.Buffer, readArguments.Offset, readArguments.Count);
                    }
                    break;
                case RoutedPipelineStreamMethod.SEEK:
                    {
                        SeekArguments seekArguments = (SeekArguments)Input.SecondaryData;
                        seekArguments.Result = BaseStream.Seek(seekArguments.Offset, seekArguments.Origin);
                    }
                    break;
                case RoutedPipelineStreamMethod.SETLENGTH:
                    BaseStream.SetLength((long)Input.SecondaryData);
                    break;
                case RoutedPipelineStreamMethod.WRITE:
                    {
                        WriteArguments writeArguments = (WriteArguments)Input.SecondaryData;
                        BaseStream.Write(writeArguments.Buffer, writeArguments.Offset, writeArguments.Count);
                    }
                    break;
                case RoutedPipelineStreamMethod.FLUSH:
                    {
                        BaseStream.Flush();
                    }
                    break;
                default:
                    Trace.WriteLine("Unknown stream operation");
                    break;
            }
            return Input;
        }
    }
}

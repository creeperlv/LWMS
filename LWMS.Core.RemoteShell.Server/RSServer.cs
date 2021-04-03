using System;
using System.Net;
using System.Net.Sockets;
using CLUNL.Pipeline;
using LWMS.Core.WR;

namespace LWMS.Core.RemoteShell.Server
{
    public class RSServer
    {
        public static string RSAPublicKey;
        public static string RSAPrivateKey;
        public Socket Listener;
        public RSServer(IPEndPoint point)
        {
            Listener = new Socket(point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(point);
        }
    }
    public class RSCMDReciver : IPipedProcessUnit
    {

        public PipelineData Process(PipelineData Input)
        {
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
                }
                else
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

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CLUNL.Pipeline;
using LWMS.Core.WR;

namespace LWMS.Core.RemoteShell.Server
{
    public class RSServer
    {
        int MaxConnections;
        int BufferSize;
        //BufferManager
        //SocketAsyncEventArgsPool eventArgsPool;
        public static string RSAPublicKey;
        public static string RSAPrivateKey;
        public Socket Listener;
        Semaphore ThreadLimitation;
        public RSServer(IPEndPoint point,int MaxConnections,int BufferSize)
        {
            Listener = new Socket(point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(point);
            this.MaxConnections = MaxConnections;
            this.BufferSize = BufferSize;
            ThreadLimitation = new Semaphore(MaxConnections, MaxConnections);
        }
        bool Stop = false;
        public void Start()
        {
            Listener.Listen(100);
            Task.Run(() => {
                while (Stop is false)
                {
                    var s=Listener.Accept();
                    var v = ValidAndLogin(s);
                    if (v.Item1)
                    {
                        ThreadLimitation.WaitOne();
                        (string, Socket) _s=(v.Item2,s);
                        RSCMDReciver.AddSocket(_s);
                    }
                }
            });
        }
        internal void Watch((string, Socket) s)
        {
            s.Item2.ReceiveTimeout =0;
        }
        internal (bool,string) ValidAndLogin(Socket client)
        {
            return (false,"");
        }
    }

    public class RSCMDReciver : IPipedProcessUnit
    {
        internal static void AddSocket((string,Socket) s)
        {

        }
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

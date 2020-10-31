using CLUNL;
using CLUNL.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace LWMS.Core.HttpRoutedLayer
{
    public class HttpListenerRoutedContext
    {
        internal HttpListenerContext CoreContext;
        public HttpListenerRequest Request { get; private set; }
        public HttpListenerRoutedResponse Response { get; private set; }
        public IPrincipal User { get; private set; }
        public HttpListenerRoutedContext(HttpListenerContext Context)
        {
            CoreContext = Context;
            Request = CoreContext.Request;
            User = CoreContext.User;
            Response = new HttpListenerRoutedResponse(this);

        }
        public async Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol)
        {
            return await CoreContext.AcceptWebSocketAsync(subProtocol);
        }
        public async Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, TimeSpan keepAliveInterval)
        {
            return await CoreContext.AcceptWebSocketAsync(subProtocol, keepAliveInterval);
        }
        public async Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, int receiveBufferSize, TimeSpan keepAliveInterval)
        {
            return await CoreContext.AcceptWebSocketAsync(subProtocol, receiveBufferSize, keepAliveInterval);
        }
        public async Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, int receiveBufferSize, TimeSpan keepAliveInterval, ArraySegment<byte> internalBuffer)
        {
            return await CoreContext.AcceptWebSocketAsync(subProtocol, receiveBufferSize, keepAliveInterval, internalBuffer);
        }
    }
    public class HttpListenerRoutedResponse : IDisposable
    {
        HttpListenerResponse CoreResponse;
        HttpListenerRoutedContext context;
        public RoutedPipelineStream OutputStream;
        public int StatusCode { get => CoreResponse.StatusCode; set => CoreResponse.StatusCode = value; }
        public long ContentLength64 { get => CoreResponse.ContentLength64; set => CoreResponse.ContentLength64 = value; }
        public string ContentType { get => CoreResponse.ContentType; set => CoreResponse.ContentType = value; }
        public Encoding ContentEncoding { get => CoreResponse.ContentEncoding; set => CoreResponse.ContentEncoding = value; }
        public WebHeaderCollection Headers { get => CoreResponse.Headers; set => CoreResponse.Headers = value; }
        public CookieCollection Cookies { get => CoreResponse.Cookies; set => CoreResponse.Cookies = value; }
        public bool KeepAlive { get => CoreResponse.KeepAlive; set => CoreResponse.KeepAlive = value; }
        public Version ProtocolVersion { get => CoreResponse.ProtocolVersion; set => CoreResponse.ProtocolVersion = value; }
        public string RedirectLocation { get => CoreResponse.RedirectLocation; set => CoreResponse.RedirectLocation = value; }
        public bool SendChunked { get => CoreResponse.SendChunked; set => CoreResponse.SendChunked = value; }
        public string StatusDescription { get => CoreResponse.StatusDescription; set => CoreResponse.StatusDescription = value; }
        public HttpListenerRoutedResponse(HttpListenerRoutedContext context)
        {
            this.context = context;
            CoreResponse = this.context.CoreContext.Response;
            OutputStream = new RoutedPipelineStream(CoreResponse.OutputStream, context);
        }
        public void Abort()
        {
            CoreResponse.Abort();
        }
        public void AddHeader(string name, string value)
        {
            CoreResponse.AddHeader(name, value);
        }
        public void AppendCookie(Cookie c)
        {
            CoreResponse.AppendCookie(c);
        }

        public void AppendHeader(string name, string value)
        {
            CoreResponse.AppendHeader(name, value);
        }
        public void Close()
        {
            CoreResponse.Close();
        }
        public void CopyForm(HttpListenerRoutedResponse response)
        {
            CoreResponse.CopyFrom(response.CoreResponse);
        }
        public void Dispose()
        {
            CoreResponse.Close();
        }
        public void Redirect(string url)
        {
            CoreResponse.Redirect(url);
        }
        public void SetCookie(Cookie cookie)
        {
            CoreResponse.SetCookie(cookie);
        }
    }
    public class RoutedPipelineStream : Stream
    {
        Stream CoreStream;
        HttpListenerRoutedContext CoreContext;
        public RoutedPipelineStream(Stream stream, HttpListenerRoutedContext Core)
        {
            CoreStream = stream;
            CoreContext = Core;
        }
        public override void Close()
        {
            base.Close();
            CoreStream.Close();
        }
        public override ValueTask DisposeAsync()
        {
            return CoreStream.DisposeAsync();
        }
        public new void Dispose()
        {
            CoreStream.Dispose();
        }
        public override bool CanRead => CoreStream.CanRead;

        public override bool CanSeek => CoreStream.CanSeek;

        public override bool CanWrite => CoreStream.CanWrite;

        public override long Length => CoreStream.Length;

        public override long Position { get => CoreStream.Position; set => CoreStream.Position = value; }

        public override void Flush()
        {
            PipelineStreamProcessor.DefaultPublicStreamProcessor.Process(new PipelineData(CoreStream, null, new RoutedPipelineStreamOptions(RoutedPipelineStreamMethod.FLUSH, CoreContext)));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int Result = 0;
            ReadArguments _result = (ReadArguments)PipelineStreamProcessor.DefaultPublicStreamProcessor.Process(
                new PipelineData(CoreStream,
                new ReadArguments(buffer, offset, count, Result), new RoutedPipelineStreamOptions(RoutedPipelineStreamMethod.READ, CoreContext))
                ).SecondaryData;
            return _result.Result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long Result = 0;
            var _result = (SeekArguments)PipelineStreamProcessor.DefaultPublicStreamProcessor.Process(new PipelineData(CoreStream, new SeekArguments(offset, origin, Result),
                new RoutedPipelineStreamOptions(RoutedPipelineStreamMethod.SEEK, CoreContext))).SecondaryData;
            return _result.Result;
        }

        public override void SetLength(long value)
        {
            PipelineStreamProcessor.DefaultPublicStreamProcessor.Process(new PipelineData(CoreStream, value, new RoutedPipelineStreamOptions(RoutedPipelineStreamMethod.SETLENGTH, CoreContext)));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            PipelineStreamProcessor.DefaultPublicStreamProcessor.Process(
                new PipelineData(
                    CoreStream,
                    new WriteArguments(buffer, offset, count), new RoutedPipelineStreamOptions(RoutedPipelineStreamMethod.WRITE, CoreContext)
                ));
        }
    }
    public class SeekArguments
    {
        public long Offset;
        public SeekOrigin Origin;
        public long Result;
        public SeekArguments(long offset, SeekOrigin origin, long result)
        {
            Offset = offset;
            Origin = origin;
            Result = result;
        }
    }
    public class ReadArguments
    {
        /// <summary>
        /// Stores read bytes.
        /// </summary>
        public byte[] Buffer;
        public int Offset;
        /// <summary>
        /// Target length to read.
        /// </summary>
        public int Count;
        /// <summary>
        /// Should be identical with the reture value of Stream.Read(...). Returns the actual length that the RoutedStream reads.
        /// </summary>
        public int Result;
        public ReadArguments(byte[] buffer, int offset, int count, int result)
        {
            Buffer = buffer;
            Offset = offset;
            Count = count;
            Result = result;
        }
    }
    public class WriteArguments
    {
        /// <summary>
        /// Stores bytes to write.
        /// </summary>
        public byte[] Buffer;
        /// <summary>
        /// Offset
        /// </summary>
        public int Offset;
        /// <summary>
        /// Length that will write.
        /// </summary>
        public int Count;
        public WriteArguments(byte[] buffer, int offset, int count)
        {
            Buffer = buffer;
            Offset = offset;
            Count = count;
        }
    }
    public class PipelineStreamProcessor : IPipelineProcessor
    {
        public static PipelineStreamProcessor DefaultPublicStreamProcessor = new PipelineStreamProcessor();
        List<IPipedProcessUnit> processUnits = new List<IPipedProcessUnit>();
        public void Init(params IPipedProcessUnit[] units)
        {
            if (units.Length == 0)
            {
                processUnits.Add(new DefaultProcessUnit());
            }
            else
            {
                processUnits = new List<IPipedProcessUnit>(units);
            }
        }
        public void Init()
        {
            Init(new IPipedProcessUnit[0]);
        }

        public void Init(ProcessUnitManifest manifest)
        {
            processUnits = manifest.GetUnitInstances();
        }
        public PipelineData Process(PipelineData Input)
        {
            bool willIgnore = false;
            try
            {
                if (LibraryInfo.GetFlag(FeatureFlags.Pipeline_IgnoreError) == 1)
                {
                    willIgnore = true;
                }
            }
            catch (Exception)
            {
                //Ignore
            }
            return Process(Input, willIgnore);
        }

        public PipelineData Process(PipelineData Input, bool IgnoreError)
        {
            if (IgnoreError)
            {

                foreach (var item in processUnits)
                {
                    try
                    {
                        var output = item.Process(Input);
                        if (Input.CheckContinuity(output))
                        {
                            Input = output;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                return Input;
            }
            else
            {

                foreach (var item in processUnits)
                {
                    var output = item.Process(Input);
                    if (Input.CheckContinuity(output))
                    {
                        Input = output;
                    }
                    else
                    {
                        throw new PipelineDataContinuityException(item);
                    }
                }
                return Input;
            }
        }
    }
    public class RoutedPipelineStreamOptions
    {
        public RoutedPipelineStreamOptions(RoutedPipelineStreamMethod method, HttpListenerRoutedContext context)
        {
            Method = method;
            Context = context;
        }
        public HttpListenerRoutedContext Context { get; internal set; }
        public RoutedPipelineStreamMethod Method { get; internal set; }
    }
    public enum RoutedPipelineStreamMethod
    {
        READ, SEEK, SETLENGTH, WRITE, FLUSH,CLOSE
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace LWMS.Core.HttpRoutedLayer
{
    public class HttpListenerRoutedContext
    {
        HttpListenerContext CoreContext;
        public HttpListenerRequest Request { get; private set; }
        public HttpListenerRoutedResponse Response { get; private set; }
        public IPrincipal User { get; private set; }
        public HttpListenerRoutedContext(HttpListenerContext Context)
        {
            CoreContext = Context;
            Request = CoreContext.Request;
            User = CoreContext.User;
            Response = new HttpListenerRoutedResponse(CoreContext.Response);

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
        public HttpListenerRoutedResponse(HttpListenerResponse response)
        {
            CoreResponse = response;
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
        public void SetCookie(Cookie cookie) { 
            CoreResponse.SetCookie(cookie);
        }

    }
}

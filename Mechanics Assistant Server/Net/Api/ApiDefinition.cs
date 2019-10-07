using System;
using System.Net;
using MechanicsAssistantServer.Data;

namespace MechanicsAssistantServer.Net.Api
{
    public delegate void HttpMessageHandler(HttpListenerContext ctxIn);

    public class ApiDefinition
    {
        protected HttpUri Uri;
        public HttpUri URI { get { return Uri; } }
        public HttpMessageHandler PUT { get; protected set; }
        public HttpMessageHandler POST { get; protected set; }
        public HttpMessageHandler GET { get; protected set; }
        public HttpMessageHandler DELETE { get; protected set; }
        public HttpMessageHandler OPTIONS { get; protected set; }
        public ApiDefinition(string baseUri)
        {
            Uri = new HttpUri(baseUri);
        }

        public void NotSupported (HttpListenerContext ctx) {
            ctx.Response.StatusCode = 405;
            ctx.Response.StatusDescription = "Method Not Supported";
            ctx.Response.OutputStream.Close();
        }
    }
}

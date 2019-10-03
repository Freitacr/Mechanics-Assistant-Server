using System;
using System.Net;

namespace MechanicsAssistantServer.Net.Api
{
    public delegate void HttpMessageHandler(HttpListenerContext ctxIn);

    public class ApiDefinition
    {
        protected string Uri;
        public string URI { get { return Uri; } }
        public HttpMessageHandler PUT { get; protected set; }
        public HttpMessageHandler POST { get; protected set; }
        public HttpMessageHandler GET { get; protected set; }
        public HttpMessageHandler DELETE { get; protected set; }
        public HttpMessageHandler OPTIONS { get; protected set; }

        public UriMappingCollection ContainedDefinition { get; private set; }
        public ApiDefinition(string baseUri)
        {
            ContainedDefinition = new UriMappingCollection();
            Uri = baseUri + "/";
        }

        public void NotSupported (HttpListenerContext ctx) {
            ctx.Response.StatusCode = 405;
            ctx.Response.StatusDescription = "Method Not Supported";
            ctx.Response.OutputStream.Close();
        }
    }
}

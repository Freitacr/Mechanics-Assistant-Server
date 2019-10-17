using System;
using System.Net;
using MechanicsAssistantServer.Data;

namespace MechanicsAssistantServer.Net.Api
{
    /**<summary>Definition of an HttpMessageHandler Method</summary>*/
    public delegate void HttpMessageHandler(HttpListenerContext ctxIn);

    /** <summary>Class that represents the definition of one part of a web api</summary> */
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

        /** <summary>Method to reliably tell the requesting client that the method they used to request a resource is not valid</summary> */
        public static void NotSupported (HttpListenerContext ctx) {
            ctx.Response.StatusCode = 405;
            ctx.Response.StatusDescription = "Method Not Supported";
            ctx.Response.OutputStream.Close();
        }
    }
}

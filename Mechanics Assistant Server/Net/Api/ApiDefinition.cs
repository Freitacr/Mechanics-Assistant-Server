using System.Text;
using System.Net;
using OldManInTheShopServer.Data;
using System;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Net.Api
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
        public HttpMessageHandler PATCH { get; protected set; }
        public ApiDefinition(string baseUri)
        {
            Uri = new HttpUri(baseUri);
            OPTIONS += HandleOptionRequest;
        }
        public void HandleOptionRequest(HttpListenerContext ctxIn)
        {
            try
            {
                ctxIn.Response.StatusCode = 200;
                ctxIn.Response.AddHeader("Access-Control-Allow-Methods", "*");
                ctxIn.Response.AddHeader("Access-Control-Allow-Origin", "*");
                ctxIn.Response.AddHeader("Access-Control-Allow-Headers", "*");
                ctxIn.Response.Close();
            }
            catch (HttpListenerException)
            {
                //HttpListenerContext objects dispose themselves on HttpListenerExceptions happening.
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctxIn, 500, "Internal Server Error", "Error occurred while responding: " + e.Message);
            }
        }

        /** <summary>Method to reliably tell the requesting client that the method they used to request a resource is not valid</summary> */
        public static void NotSupported (HttpListenerContext ctx) {
            try
            {
                ctx.Response.StatusCode = 405;
                ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");
                ctx.Response.AddHeader("Access-Control-Allow-Headers", "*");
                ctx.Response.StatusDescription = "Method Not Supported";
                ctx.Response.OutputStream.Close();
            }
            catch (HttpListenerException)
            {
                //HttpListenerContext objects dispose themselves on HttpListenerExceptions happening.
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occurred while responding: " + e.Message);
            }
        }

        public static void WriteBodyResponse(HttpListenerContext ctx, int responseCode, string responseString, string responseBody, string contentType = "text/plain")
        {
            try
            {
                ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");
                ctx.Response.AddHeader("Access-Control-Allow-Headers", "*");
                ctx.Response.StatusCode = responseCode;
                ctx.Response.StatusDescription = responseString;
                ctx.Response.ContentType = contentType;
                byte[] resp = Encoding.UTF8.GetBytes(responseBody);
                ctx.Response.ContentLength64 = resp.LongLength;
                ctx.Response.OutputStream.Write(resp, 0, resp.Length);
                ctx.Response.OutputStream.Close();
            } catch (HttpListenerException)
            {
                //HttpListenerContext objects dispose themselves on HttpListenerExceptions happening.
            } catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occurred while responding: " + e.Message);
            }
        }

        public static void WriteBodylessResponse(HttpListenerContext ctx, int responseCode, string responseString)
        {
            try
            {
                ctx.Response.StatusCode = responseCode;
                ctx.Response.StatusDescription = responseString;
                ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");
                ctx.Response.AddHeader("Access-Control-Allow-Headers", "*");
                ctx.Response.OutputStream.Close();
            }
            catch (HttpListenerException)
            {
                //HttpListenerContext objects dispose themselves on HttpListenerExceptions happening.
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occurred while responding: " + e.Message);
            }
        }
    }
}

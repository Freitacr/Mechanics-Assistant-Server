using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace MechanicsAssistantServer.Net
{
    public class QueryResponseServer
    {
        private HttpListener Listener;
        private UriMappingCollection PrefixMapping;
        public bool IsAlive { get; private set; }
       
        public QueryResponseServer()
        {

        }

        public void ListenForResponses(UriMappingCollection prefixMapping)
        {
            Listener = new HttpListener();
            foreach (KeyValuePair<string, Api.ApiDefinition> prefixPair in prefixMapping)
            {
                if (!Listener.Prefixes.Contains(prefixPair.Key))
                    Listener.Prefixes.Add(prefixPair.Key);
            }
            PrefixMapping = prefixMapping;

            Listener.Start();
            IsAlive = true;
            ThreadPool.QueueUserWorkItem(
            (unused) => {
                Console.WriteLine("Server has begun listening for responses...");
                while (Listener.IsListening)
                {
                    var ctx = Listener.GetContext();
                    if (ctx == null)
                        continue;
                    string method = ctx.Request.HttpMethod;
                    string uri = ctx.Request.Url.ToString();
                    if (!uri.EndsWith('/'))
                        uri += '/';
                    var action = PrefixMapping[uri];
                    if (action == null)
                    {
                        //Assume unsupported operation
                        ctx.Response.StatusCode = 404;
                        ctx.Response.StatusDescription = "Not Found";
                        ctx.Response.OutputStream.Close();
                    } else
                    {
                        switch(method)
                        {
                            case "DELETE":
                                ThreadPool.QueueUserWorkItem(
                                    (context) => action.DELETE(context as HttpListenerContext), ctx
                                );
                                break;
                            case "GET":
                                ThreadPool.QueueUserWorkItem(
                                    (context) => action.GET(context as HttpListenerContext), ctx
                                );
                                break;
                            case "PUT":
                                ThreadPool.QueueUserWorkItem(
                                    (context) => action.PUT(context as HttpListenerContext), ctx
                                );
                                break;
                            case "POST":
                                ThreadPool.QueueUserWorkItem(
                                    (context) => action.POST(context as HttpListenerContext), ctx
                                );
                                break;
                            case "OPTIONS":
                                ThreadPool.QueueUserWorkItem(
                                    (context) => action.OPTIONS(context as HttpListenerContext), ctx
                                );
                                break;
                            default:
                                ctx.Response.StatusCode = 405;
                                ctx.Response.StatusDescription = "Method Not Supported";
                                ctx.Response.OutputStream.Close();
                                break;
                        }
                    }
                    //Console.WriteLine("HTTP request came in: " + ctx.Request.HttpMethod + " " + ctx.Request.Url);
                }
            }
            );

        }

        public void Close()
        {
            if (Listener.IsListening)
            {
                Listener.Stop();
                Listener.Close();
                IsAlive = false;
            }
        }
    }
}

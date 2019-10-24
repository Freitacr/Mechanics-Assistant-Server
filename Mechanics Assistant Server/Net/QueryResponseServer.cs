using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using OldManInTheShopServer.Net.Api;

namespace OldManInTheShopServer.Net
{
    public class QueryResponseServer
    {
        private HttpListener Listener;

        public bool IsAlive { get; private set; }
       
        public QueryResponseServer()
        {

        }

        public void ListenForResponses(UriMappingCollection prefixMapping)
        {
            Listener = new HttpListener();
            prefixMapping.AddPrefixes(Listener);
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
                    Console.WriteLine("Received request for " + uri);
                    Console.WriteLine("Request came from " + ctx.Request.RemoteEndPoint.ToString());
                    if (!uri.EndsWith('/'))
                        uri += '/';
                    var action = prefixMapping[uri];
                    if (action == null)
                    {
                        //Assume unsupported operation
                        ctx.Response.StatusCode = 404;
                        ctx.Response.StatusDescription = "Not Found";
                        ctx.Response.OutputStream.Close();
                    } else
                    {
                        HttpMessageHandler handler = null;
                        switch(method)
                        {
                            case "DELETE":
                                handler = action.DELETE;
                                break;
                            case "GET":
                                handler = action.GET;
                                break;
                            case "PUT":
                                handler = action.PUT;
                                break;
                            case "POST":
                                handler = action.POST;
                                break;
                            case "OPTIONS":
                                handler = action.OPTIONS;
                                break;
                            case "PATCH":
                                handler = action.PATCH;
                                break;
                        }
                        if (handler == null)
                            ThreadPool.QueueUserWorkItem((context) => ApiDefinition.NotSupported(context as HttpListenerContext), ctx);
                        else
                            ThreadPool.QueueUserWorkItem((context) => handler(context as HttpListenerContext), ctx);
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

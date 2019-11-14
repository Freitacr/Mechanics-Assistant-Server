using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using OldManInTheShopServer.Net.Api;
using OldManInTheShopServer.Util;

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
                    Console.WriteLine("Api Handling the request: " + action.GetType().Name);
                    if (action == null)
                    {
                        //Assume unsupported operation
                        try
                        {
                            ctx.Response.StatusCode = 404;
                            ctx.Response.StatusDescription = "Not Found";
                            ctx.Response.OutputStream.Close();
                        }
                        catch (HttpListenerException)
                        {
                            //HttpListeners dispose themselves when an exception occurs, so we can do no more.
                        }
                    } else
                    {
                        HttpMessageHandler handler = null;
                        string responseMethod = null;
                        switch(method)
                        {
                            case "DELETE":
                                responseMethod = "DELETE";
                                handler = action.DELETE;
                                break;
                            case "GET":
                                responseMethod = "GET";
                                handler = action.GET;
                                break;
                            case "PUT":
                                responseMethod = "PUT";
                                handler = action.PUT;
                                break;
                            case "POST":
                                responseMethod = "POST";
                                handler = action.POST;
                                break;
                            case "OPTIONS":
                                responseMethod = "OPTIONS";
                                handler = action.OPTIONS;
                                break;
                            case "PATCH":
                                responseMethod = "PATCH";
                                handler = action.PATCH;
                                break;
                        }
                        if (handler == null)
                        {
                            try
                            {
                                Console.WriteLine("Telling the client that the operation they attempted was not supported");
                                ThreadPool.QueueUserWorkItem((context) => ApiDefinition.NotSupported(context as HttpListenerContext), ctx);
                            } catch (Exception e)
                            {
                                Logger.Global.Log(Logger.LogLevel.ERROR, e.StackTrace);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Responding with a " + responseMethod);
                            ThreadPool.QueueUserWorkItem((context) => handler(context as HttpListenerContext), ctx);
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

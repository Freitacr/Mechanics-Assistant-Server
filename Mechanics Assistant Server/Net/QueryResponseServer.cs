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
            foreach (KeyValuePair<UriMapping, Action<HttpListenerContext>> prefixPair in prefixMapping)
            {
                if (!Listener.Prefixes.Contains(prefixPair.Key.Uri))
                    Listener.Prefixes.Add(prefixPair.Key.Uri);
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
                    UriMapping mapping = new UriMapping(method, uri);
                    var action = PrefixMapping[mapping];
                    if (action == null)
                    {
                        //Assume unsupported operation
                        ctx.Response.StatusCode = 405;
                        ctx.Response.StatusDescription = "Method Unsupported";
                        ctx.Response.OutputStream.Close();
                    } else
                    {
                        ThreadPool.QueueUserWorkItem(
                            (context) => action(context as HttpListenerContext), ctx
                        );
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

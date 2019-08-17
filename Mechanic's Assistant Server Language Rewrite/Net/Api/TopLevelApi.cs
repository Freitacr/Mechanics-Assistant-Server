using System;
using System.Text;
using System.Net;

namespace MechanicsAssistantServer.Net.Api
{
    public class TopLevelApi : ApiDefinition
    {
        private QueryResponseServer Server;
        public TopLevelApi(int port, QueryResponseServer serverIn) : base("http://localhost:" + port)
        {
            Server = serverIn;
            AddAction("delete", ShutdownServer);
        }

        public void ShutdownServer(HttpListenerContext ctxIn)
        {
            ctxIn.Response.StatusCode = 200;
            ctxIn.Response.StatusDescription = "OK";
            ctxIn.Response.ContentType = "text/plain";
            string resp = "Attempting to close server...";
            byte[] toWrite = Encoding.UTF8.GetBytes(resp);
            ctxIn.Response.ContentLength64 = toWrite.LongLength;
            ctxIn.Response.OutputStream.Write(toWrite, 0, toWrite.Length);
            ctxIn.Response.OutputStream.Close();
            Server.Close();
        }
    }
}

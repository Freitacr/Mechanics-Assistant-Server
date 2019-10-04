using System.Text;
using System.Net;

namespace MechanicsAssistantServer.Net.Api
{
    public class TopLevelApi : ApiDefinition
    {
        private QueryResponseServer Server;
        public TopLevelApi(int port, QueryResponseServer serverIn) : base("https://+:" + port)
        {
            Server = serverIn;
            DELETE += ShutdownServer;
            GET += NotSupported;
            POST += NotSupported;
            PUT += NotSupported;
            OPTIONS += HandleOptionRequest;
        }

        public void HandleOptionRequest(HttpListenerContext ctxIn)
        {
            ctxIn.Response.StatusCode = 200;
            ctxIn.Response.AddHeader("Access-Control-Allow-Methods", "DELETE");
            ctxIn.Response.AddHeader("Access-Control-Allow-Origin", "*");
            ctxIn.Response.AddHeader("Access-Control-Allow-Headers", "*");
            ctxIn.Response.Close();
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

using System.Text;
using System.Net;

namespace MechanicsAssistantServer.Net.Api
{
    public class TopLevelApi : ApiDefinition
    {
        public TopLevelApi() : base("http://+")
        {
            DELETE += NotSupported;
            GET += SendRedirect;
            POST += NotSupported;
            PUT += NotSupported;
            OPTIONS += HandleOptionRequest;
        }

        public void HandleOptionRequest(HttpListenerContext ctxIn)
        {
            ctxIn.Response.StatusCode = 200;
            ctxIn.Response.AddHeader("Access-Control-Allow-Methods", "GET");
            ctxIn.Response.AddHeader("Access-Control-Allow-Origin", "*");
            ctxIn.Response.AddHeader("Access-Control-Allow-Headers", "*");
            ctxIn.Response.Close();
        }

        public void SendRedirect(HttpListenerContext ctxIn)
        {
            string html = "<html><head><meta http-equiv=\"Refresh\" content=\"0; url=https://oldmanintheshop.web.app\"></head><body></body></html>";
            byte[] htmlBytes = Encoding.UTF8.GetBytes(html);
            ctxIn.Response.ContentType = "text/html";
            ctxIn.Response.StatusCode = 200;
            ctxIn.Response.ContentLength64 = htmlBytes.Length;
            ctxIn.Response.OutputStream.Write(htmlBytes, 0, htmlBytes.Length);
            ctxIn.Response.Close();
        }
    }
}

using System.Text;
using System.Net;
using System;

namespace OldManInTheShopServer.Net.Api
{
    /**<summary>Handles redirecting clients that are requesting web pages not already covered by another http api</summary>*/
    public class TopLevelApi : ApiDefinition
    {
        public TopLevelApi() : base("http://+")
        {
            GET += SendRedirect;
        }

        public void SendRedirect(HttpListenerContext ctxIn)
        {
            try
            {
                string html = "<html><head><meta http-equiv=\"Refresh\" content=\"0; url=https://oldmanintheshop.web.app\"></head><body></body></html>";
                WriteBodyResponse(ctxIn, 200, "OK", html, "text/html");
            } catch(Exception e)
            {
                WriteBodyResponse(ctxIn, 500, "Internal Server Error", e.Message);
            }
        }
    }
}

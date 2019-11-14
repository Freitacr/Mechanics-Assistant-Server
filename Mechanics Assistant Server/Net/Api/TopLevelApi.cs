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
                byte[] htmlBytes = Encoding.UTF8.GetBytes(html);
                ctxIn.Response.ContentType = "text/html";
                ctxIn.Response.StatusCode = 200;
                ctxIn.Response.ContentLength64 = htmlBytes.Length;
                ctxIn.Response.OutputStream.Write(htmlBytes, 0, htmlBytes.Length);
                ctxIn.Response.Close();
            }
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
            } catch (Exception)
            {
                ctxIn.Response.Close();
            }
        }
    }
}

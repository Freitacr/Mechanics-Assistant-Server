using System;
using System.Text;
using System.Net;
using System.IO;

namespace OldManInTheShopServer.Net.Api
{
    /**<summary>Api defintion for responding to the HTTP challenge made by the Lets Encrypt ACME Api</summary>*/
    class CertValidationApi : ApiDefinition
    {
        public CertValidationApi() : base("http://+/.well-known/acme-challenge")
        {
            GET += HandleGetRequest;
            OPTIONS += HandleOptionRequest;
            //AddAction("put", HandlePutRequest);
            //AddAction("post", HandlePostRequest);
        }

        public void HandleOptionRequest(HttpListenerContext ctxIn)
        {
            try
            {
                ctxIn.Response.StatusCode = 200;
                ctxIn.Response.AddHeader("Access-Control-Allow-Methods", "GET");
                ctxIn.Response.AddHeader("Access-Control-Allow-Origin", "*");
                ctxIn.Response.AddHeader("Access-Control-Allow-Headers", "*");
                ctxIn.Response.Close();
            }
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
            }
        }


        /**<summary>Responds to the http challenge by sending the contents of the file that was requested.
         * This is secure because the only entities that know the token (what the file is name) is our server
         * and the Let's Encrypt server</summary>*/
        public void HandleGetRequest(HttpListenerContext ctx)
        {
            try
            {
                string fileName = ctx.Request.RawUrl;

                int challengeIndex = fileName.IndexOf("acme-challenge/");
                int fileEnd = fileName.IndexOf('/', challengeIndex + 15);
                if (fileEnd != -1)
                {
                    ctx.Response.StatusCode = 404;
                    ctx.Response.StatusDescription = "Not Found";
                    ctx.Response.OutputStream.Close();
                    return;
                }
                fileName = fileName.Substring(challengeIndex + 15);
                StreamReader reader;
                try
                {
                    reader = new StreamReader("tokens/" + fileName);
                }
                catch (FileNotFoundException)
                {
                    ctx.Response.StatusCode = 404;
                    ctx.Response.StatusDescription = "Not Found";
                    ctx.Response.OutputStream.Close();
                    return;
                }
                string authzToken = reader.ReadToEnd();
                reader.Close();
                byte[] token = Encoding.UTF8.GetBytes(authzToken);
                ctx.Response.ContentType = "application/octet-stream";
                ctx.Response.ContentLength64 = token.Length;
                ctx.Response.OutputStream.Write(token, 0, token.Length);
                ctx.Response.Close();
            }
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
            } catch (Exception)
            {
                ctx.Response.Close();
            }
        }
    }
}

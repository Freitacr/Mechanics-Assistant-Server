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
                WriteBodyResponse(ctx, 200, "OK", authzToken, "application/octet-stream");
            } catch(Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }
    }
}

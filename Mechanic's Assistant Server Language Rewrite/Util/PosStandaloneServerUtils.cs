using System.Diagnostics;
using System;
using System.IO;
using System.Threading;
using System.Net.Http;
using System.Net;
using System.Text;

namespace MechanicsAssistantServer.Util
{
    public class PosStandaloneServerUtils
    {
        public static HttpClient HttpClient { get; private set; } = new HttpClient();

        public static void StartupPosTaggerServer()
        {
            Process.Start("PosTagger.exe");
            Thread.Sleep(5000);
        }

        public static string AskServerAboutSentence(string sentenceIn)
        {
            WebRequest req = WebRequest.Create("http://localhost:21569/");
            req.UseDefaultCredentials = true;
            req.ContentType = "text/plain";
            req.Method = "PUT";
            req.Credentials = CredentialCache.DefaultCredentials;
            byte[] encodedSentence = Encoding.UTF8.GetBytes(sentenceIn);
            req.ContentLength = encodedSentence.Length;
            req.GetRequestStream().Write(encodedSentence, 0, (int)req.ContentLength);
            //req.GetRequestStream().Close();
            try
            {
                WebResponse resp = req.GetResponse();
                StreamReader reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                return reader.ReadToEnd();
            } catch(WebException e)
            {
                using (WebResponse resp = e.Response)
                {
                    Console.WriteLine(resp);
                    return null;
                }
            }
        }

        public static void ShutdownPosTaggerServerAsync()
        {
            WebRequest shutdownRequest = WebRequest.Create("http://localhost:21569/");
            shutdownRequest.UseDefaultCredentials = true;
            shutdownRequest.Method = "POST";
            shutdownRequest.GetResponse();
        }
    }
}

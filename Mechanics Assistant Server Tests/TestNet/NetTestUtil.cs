using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using OldManInTheShopServer.Util;
using OldManInTheShopServer.Data;


namespace MechanicsAssistantServerTests.TestNet
{

    class ServerTestingMessageSwitchback
    {
        private static HttpListener Switchback = null;
        private static readonly int DEFAULT_SWITCHBACK_PORT = 2400;
        private static string SwitchbackUri = null;

        public static HttpListener GetSwitchback()
        {
            if(Switchback == null)
            {
                Switchback = new HttpListener();
                SwitchbackUri = "http://+:" + DEFAULT_SWITCHBACK_PORT;
                HttpUri switchbackUri = new HttpUri(SwitchbackUri);
                Switchback.Prefixes.Add(switchbackUri.Prefix);
                Switchback.Start();
            }
            return Switchback;
        }

        public static void GetResponseCallback(IAsyncResult resultIn)
        {
            Console.WriteLine(resultIn.CompletedSynchronously);
            Console.WriteLine(resultIn.IsCompleted);
        }

        public static object[] SwitchbackMessage(JsonDictionaryStringConstructor messageContentsIn, string httpRequestMethod)
        {
            var switchback = GetSwitchback();
            var switchbackUri = SwitchbackUri.Replace("+", "localhost");
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(switchbackUri);
            req.Method = httpRequestMethod;
            if (!req.Method.Equals("GET"))
            {
                Stream messageStream = req.GetRequestStream();
                byte[] messageBytes = Encoding.UTF8.GetBytes(messageContentsIn.ToString());
                req.ContentLength = messageBytes.Length;
                messageStream.Write(messageBytes, 0, messageBytes.Length);
            }
            var asyncState = req.BeginGetResponse(GetResponseCallback, null);
            return new object[] { switchback.GetContext(), req, asyncState};
        }

        public static void CloseSwitchback()
        {
            if(Switchback != null)
            {
                Switchback.Close();
                Switchback = null;
            }
        }
    }
}

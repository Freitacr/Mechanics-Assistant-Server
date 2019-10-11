using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace MechanicsAssistantServerTests.TestNet.TestApi
{
    [TestClass]
    public class TestQueryAPI
    {
        private static HttpClient Client;

        [ClassInitialize]
        public static void SetupClient(TestContext ctx)
        {
            Client = new HttpClient();
        }

        [TestMethod]
        public void TestPUT()
        {
            string json = "{\"make\": \"autocar\", \"model\": \"xpeditor\", \"complaint\": \"runs rough\"}";
            Client.DefaultRequestHeaders.Add("Accept", "application/json");
            var res = Client.PutAsync("https://jcf-ai.com:16384/query", new StringContent(json, Encoding.UTF8, "application/json"));
            HttpResponseMessage msg = res.Result;
            Assert.AreEqual(msg.StatusCode, System.Net.HttpStatusCode.OK);
            Assert.AreNotEqual(msg.Content.ReadAsStringAsync().Result.Length, 0);
        }
    }
}

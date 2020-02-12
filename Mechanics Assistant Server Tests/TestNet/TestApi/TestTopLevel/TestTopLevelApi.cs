using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Net.Api;
using OldManInTheShopServer.Util;

namespace MechanicsAssistantServerTests.TestNet.TestApi.TestTopLevel
{
    [TestClass]
    public class TestTopLevelApi
    {

        private static TopLevelApi TestApi;
        private static readonly string ExpectedHtml = "<html><head><meta http-equiv=\"Refresh\" content=\"0; url=https://oldmanintheshop.web.app\"></head><body></body></html>";

        [ClassInitialize]
        public static void InitializeClass(TestContext ctx)
        {
            TestApi = new TopLevelApi();
        }

        [ClassCleanup]
        public static void CleanupTests()
        {
            ServerTestingMessageSwitchback.CloseSwitchback();
        }

        [TestMethod]
        public void TestRetrieveRedirectHtml()
        {
            object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                new JsonDictionaryStringConstructor(), "GET");
            var ctx = contextAndRequest[0] as HttpListenerContext;
            var req = contextAndRequest[1] as HttpWebRequest;
            TestApi.GET(ctx);
            HttpWebResponse resp = null;
            try
            {
                resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            using (resp)
            {
                Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
                byte[] data = new byte[resp.ContentLength];
                resp.GetResponseStream().Read(data, 0, data.Length);
                string received = Encoding.UTF8.GetString(data);
                Assert.AreEqual(ExpectedHtml, received);
            }
        }


    }
}

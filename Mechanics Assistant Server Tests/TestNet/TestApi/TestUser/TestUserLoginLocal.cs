using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManInTheShopServer.Net.Api;
using System.Net;
using OldManInTheShopServer.Data.MySql;

namespace MechanicsAssistantServerTests.TestNet.TestApi.TestUser
{
    [TestClass]
    public class TestUserLoginLocal
    {

        private static UserApi TestApi;

        [ClassInitialize]
        public static void SetupTests(TestContext ctx)
        {
            if (!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initialize database schema. See logged error for details");
            TestApi = new UserApi(10000);
            if (!TestingDatabaseCreationUtils.InitializeUsers())
                throw new Exception("Failed to initialize users in database. See logged error for details");
        }

        [ClassCleanup]
        public static void CleanupTests()
        {
            ServerTestingMessageSwitchback.CloseSwitchback();
            if (!TestingDatabaseCreationUtils.DestoryDatabase())
                throw new Exception("Failed to destroy testing datbase. This is bad. Manual cleanup is required");
        }

        [TestMethod]
        public void TestLoginUser()
        {
            object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                TestingUserStorage.ValidUser1.ConstructLoginRequest(),
                "PUT");
            var ctx = contextAndRequest[0] as HttpListenerContext;
            var req = contextAndRequest[1] as HttpWebRequest;
            TestApi.PUT(ctx);

            HttpWebResponse resp;
            try
            {
                resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
            } catch (WebException e)
            {
                resp = e.Response as HttpWebResponse;
            }
            try
            {
                Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
            } catch (AssertFailedException e)
            {
                byte[] respData = new byte[resp.ContentLength];
                resp.GetResponseStream().Read(respData, 0, respData.Length);
                Console.WriteLine(Encoding.UTF8.GetString(respData));
                throw e;
            }

        }
    }
}

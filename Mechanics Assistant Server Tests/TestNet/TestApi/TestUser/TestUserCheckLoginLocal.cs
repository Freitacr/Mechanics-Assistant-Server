using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Net.Api;
using OldManInTheShopServer.Util;

namespace MechanicsAssistantServerTests.TestNet.TestApi.TestUser
{
    [TestClass]
    public class TestUserCheckLoginLocal
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
                throw new Exception("Failed to destroy testing database. This is bad. Manual cleanup is required");
        }

        [TestMethod]
        public void TestCheckLoginStatus()
        {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            OverallUser user = null;
            using (manipulator)
            {
                try
                {
                    manipulator.Connect(TestingConstants.ConnectionString);

                    Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser1));

                    user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                    var loginToken = UserVerificationUtil.ExtractLoginTokens(user);

                    //Check Login Status
                    object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                        TestingUserStorage.ValidUser1.ConstructCheckLoginStatusRequest(
                            user.UserId, loginToken.LoginToken),
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
                        byte[] respData = new byte[resp.ContentLength];
                        resp.GetResponseStream().Read(respData, 0, respData.Length);
                        Console.WriteLine(Encoding.UTF8.GetString(respData));
                        throw e;
                    }
                    Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
                }
                finally
                {
                    manipulator.UpdateUsersLoginToken(user, new LoginStatusTokens());
                }
            }
        }

        [TestMethod]
        public void TestBadRequestOnBadUserId()
        {
            object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                        TestingUserStorage.ValidUser1.ConstructCheckLoginStatusRequest(0, "x'acbad13475adbasbsdsa'"),
                        "PUT");
            var ctx = contextAndRequest[0] as HttpListenerContext;
            var req = contextAndRequest[1] as HttpWebRequest;
            TestApi.PUT(ctx);
            HttpWebResponse resp;
            try
            {
                resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                Assert.Fail("Expected a failed response, but this did not occur");
            } catch (WebException e)
            {
                resp = e.Response as HttpWebResponse;
            }
            Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [TestMethod]
        public void TestBadRequestOnEmptyLoginToken()
        {
            object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                        TestingUserStorage.ValidUser1.ConstructCheckLoginStatusRequest(1, ""),
                        "PUT");
            var ctx = contextAndRequest[0] as HttpListenerContext;
            var req = contextAndRequest[1] as HttpWebRequest;
            TestApi.PUT(ctx);
            HttpWebResponse resp;
            try
            {
                resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                Assert.Fail("Expected a failed response, but this did not occur");
            }
            catch (WebException e)
            {
                resp = e.Response as HttpWebResponse;
            }
            Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentUser()
        {
            object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                        TestingUserStorage.ValidUser1.ConstructCheckLoginStatusRequest(4, "x'acbad13475adbasbsdsa'"),
                        "PUT");
            var ctx = contextAndRequest[0] as HttpListenerContext;
            var req = contextAndRequest[1] as HttpWebRequest;
            TestApi.PUT(ctx);
            HttpWebResponse resp;
            try
            {
                resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                Assert.Fail("Expected a failed response, but this did not occur");
            }
            catch (WebException e)
            {
                resp = e.Response as HttpWebResponse;
            }
            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }
    }
}

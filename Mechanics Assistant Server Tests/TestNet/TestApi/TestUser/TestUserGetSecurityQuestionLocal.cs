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
    public class TestUserGetSecurityQuestionLocal
    {
        
        private static UserAuthApi TestApi;

        [ClassInitialize]
        public static void SetupTests(TestContext ctx)
        {
            if(!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initialize database schema. See logged error for details");
            TestApi = new UserAuthApi(10000);
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
        public void TestValidRequest()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser1));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructSecurityQuestionRequest(loginTokens.LoginToken, user.UserId),
                    "POST");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp;
                TestApi.POST(ctx);
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
                byte[] data = new byte[resp.ContentLength];
                resp.GetResponseStream().Read(data, 0, data.Length);
                string receivedData = Encoding.UTF8.GetString(data);
                Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
                Assert.AreEqual(TestingUserStorage.ValidUser1.SecurityQuestion, receivedData);

            }
        }

        [TestMethod]
        public void TestBadFormatOnEmptyLoginToken()
        {
            object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                TestingUserStorage.ValidUser1.ConstructSecurityQuestionRequest("", 1),
                "POST");
            var ctx = contextAndRequest[0] as HttpListenerContext;
            var req = contextAndRequest[1] as HttpWebRequest;
            HttpWebResponse resp;
            TestApi.POST(ctx);
            try
            {
                resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                Assert.Fail("Expected an error message, but didn't receive one.");
            }
            catch (WebException e)
            {
                resp = e.Response as HttpWebResponse;
            }
            Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [TestMethod]
        public void TestBadFormatOnBadUserId()
        {
            object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                TestingUserStorage.ValidUser1.ConstructSecurityQuestionRequest("x'ababaabbabababbbaba'", 0),
                "POST");
            var ctx = contextAndRequest[0] as HttpListenerContext;
            var req = contextAndRequest[1] as HttpWebRequest;
            HttpWebResponse resp;
            TestApi.POST(ctx);
            try
            {
                resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                Assert.Fail("Expected an error message, but didn't receive one.");
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
                TestingUserStorage.ValidUser1.ConstructSecurityQuestionRequest("x'abababbbabbbaaababa'", 5),
                "POST");
            var ctx = contextAndRequest[0] as HttpListenerContext;
            var req = contextAndRequest[1] as HttpWebRequest;
            HttpWebResponse resp;
            TestApi.POST(ctx);
            try
            {
                resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                Assert.Fail("Expected an error message, but didn't receive one.");
            }
            catch (WebException e)
            {
                resp = e.Response as HttpWebResponse;
            }
            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public void TestNotAuthorizedOnNonLoggedUser()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructSecurityQuestionRequest("x'abaababaaababaaba'", user.UserId),
                    "POST");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error message, but didn't receive one.");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
            }
        }

    }
}

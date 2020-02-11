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
    public class TestUserAuthenticationLocal
    {

        private static UserAuthApi TestApi;

        [ClassInitialize]
        public static void SetupTests(TestContext ctx)
        {
            if (!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
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
                throw new Exception("Failed to destroy testing database. This is bad. Manual cleanup is required");
        }

        [TestMethod]
        public void TestValidAuthenticationRequest()
        {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            using (manipulator)
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser2));
                OverallUser user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser2.Email))[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                var authReq = TestingUserStorage.ValidUser2.ConstructAuthenticationRequest(loginTokens.LoginToken, user.UserId);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(authReq, "PUT");
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
        }

        [TestMethod]
        public void TestBadRequestOnEmptyLoginToken()
        {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            using (manipulator)
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser2));
                OverallUser user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser2.Email))[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                var authReq = TestingUserStorage.ValidUser2.ConstructAuthenticationRequest("", user.UserId);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(authReq, "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
                HttpWebResponse resp;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail();
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnInvalidUserId()
        {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            using (manipulator)
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser2));
                OverallUser user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser2.Email))[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                var authReq = TestingUserStorage.ValidUser2.ConstructAuthenticationRequest(loginTokens.LoginToken, 0);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(authReq, "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
                HttpWebResponse resp;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail();
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnInvalidSecurityQuestion()
        {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            using (manipulator)
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser2));
                OverallUser user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser2.Email))[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                var authReq = TestingUserStorage.ValidUser2.ConstructAuthenticationRequest(loginTokens.LoginToken, user.UserId);
                authReq.SetMapping("SecurityQuestion", "");
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(authReq, "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
                HttpWebResponse resp;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail();
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnInvalidSecurityAnswer()
        {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            using (manipulator)
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser2));
                OverallUser user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser2.Email))[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                var authReq = TestingUserStorage.ValidUser2.ConstructAuthenticationRequest(loginTokens.LoginToken, user.UserId);
                authReq.SetMapping("SecurityAnswer", "");
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(authReq, "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
                HttpWebResponse resp;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail();
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentUser()
        {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            using (manipulator)
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser2));
                OverallUser user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser2.Email))[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                var authReq = TestingUserStorage.ValidUser2.ConstructAuthenticationRequest(loginTokens.LoginToken, 1000486);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(authReq, "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
                HttpWebResponse resp;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail();
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestUnauthorizedOnNonLoggedInUser()
        {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            using (manipulator)
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser2));
                OverallUser user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser2.Email))[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                var authReq = TestingUserStorage.ValidUser2.ConstructAuthenticationRequest("x'abaabacaabadadbaaadbaad'", user.UserId);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(authReq, "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
                HttpWebResponse resp;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail();
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestUnauthorizedOnIncorrectSecurityQuestion()
        {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            using (manipulator)
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser2));
                OverallUser user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser2.Email))[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                var authReq = TestingUserStorage.ValidUser2.ConstructAuthenticationRequest(loginTokens.LoginToken, user.UserId);
                authReq.SetMapping("SecurityQuestion", "why?");
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(authReq, "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
                HttpWebResponse resp;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail();
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestUnauthorizedOnIncorrectSecurityAnswer()
        {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            using (manipulator)
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser2));
                OverallUser user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser2.Email))[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                var authReq = TestingUserStorage.ValidUser2.ConstructAuthenticationRequest(loginTokens.LoginToken, user.UserId);
                authReq.SetMapping("SecurityAnswer", "a");
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(authReq, "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
                HttpWebResponse resp;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail();
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

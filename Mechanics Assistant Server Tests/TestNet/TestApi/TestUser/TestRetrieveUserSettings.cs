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
    public class TestRetrieveUserSettings
    {

        private static UserSettingsApi TestApi;

        [ClassInitialize]
        public static void InitializeClass(TestContext ctx)
        {
            if (!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initialize database schema. See logged error for details");
            TestApi = new UserSettingsApi(10000);
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
        public void TestRetrieveUserSettingsDefaultValues()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser1));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                var currentSettings = user.Settings;
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructRetrieveSettingsRequest(
                        user.UserId,
                        UserVerificationUtil.ExtractLoginTokens(user).LoginToken), 
                    "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
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
                    Assert.AreEqual(currentSettings, received);
                }
            }
        }

        [TestMethod]
        public void TestRetrieveUserSettingsModifiedSettings()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser1));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                user.UpdateSettings(UserSettingsEntryKeys.DisplayName, "New Name!");
                manipulator.UpdateUsersSettings(user);
                var currentSettings = user.Settings;
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructRetrieveSettingsRequest(
                        user.UserId,
                        UserVerificationUtil.ExtractLoginTokens(user).LoginToken),
                    "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
                user.Settings = OverallUser.GenerateDefaultSettings();
                Assert.IsTrue(manipulator.UpdateUsersSettings(user));
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
                    Assert.AreEqual(currentSettings, received);
                }
                
            }
        }

        [TestMethod]
        public void TestBadRequestOnInvalidUserId()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser1));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                var currentSettings = user.Settings;
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructRetrieveSettingsRequest(
                        0,
                        UserVerificationUtil.ExtractLoginTokens(user).LoginToken),
                    "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
                HttpWebResponse resp = null;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected error response, but did not receive one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyLoginToken()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser1));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                var currentSettings = user.Settings;
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructRetrieveSettingsRequest(
                        user.UserId,
                        "x''"),
                    "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
                HttpWebResponse resp = null;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected error response, but did not receive one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestUnauthorizedOnNonLoggedInUser()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser1));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                var currentSettings = user.Settings;
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructRetrieveSettingsRequest(
                        user.UserId,
                        "x'abaabcaba'"),
                    "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
                HttpWebResponse resp = null;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected error response, but did not receive one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentUser()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser1));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                var currentSettings = user.Settings;
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructRetrieveSettingsRequest(
                        100000,
                        UserVerificationUtil.ExtractLoginTokens(user).LoginToken),
                    "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
                HttpWebResponse resp = null;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected error response, but did not receive one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
            }
        }



    }
}

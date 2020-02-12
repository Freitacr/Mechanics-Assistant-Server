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
    public class TestUpdateUserSettingValue
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
        public void TestUpdateUserSetting()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                user.Settings = OverallUser.GenerateDefaultSettings();
                Assert.IsTrue(manipulator.UpdateUsersSettings(user));
                var currentSettings = user.Settings;
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructChangeSettingRequest(
                        user.UserId,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        UserSettingsEntryKeys.DisplayName,
                        "New Name #2!"),
                    "PATCH");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(ctx);
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
                    Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
                user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                var newSettings = JsonDataObjectUtil<List<UserSettingsEntry>>.ParseObject(user.Settings);
                foreach (UserSettingsEntry entry in newSettings) { 
                    if (entry.Key == UserSettingsEntryKeys.DisplayName)
                    {
                        Assert.AreEqual("New Name #2!", entry.Value);
                        break;
                    }
                }
            }
        }

        [TestMethod]
        public void TestBadRequestOnInvalidUserId()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                user.Settings = OverallUser.GenerateDefaultSettings();
                Assert.IsTrue(manipulator.UpdateUsersSettings(user));
                var currentSettings = user.Settings;
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructChangeSettingRequest(
                        0,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        UserSettingsEntryKeys.DisplayName,
                        "New Name #2!"),
                    "PATCH");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(ctx);
                HttpWebResponse resp = null;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                using (resp)
                    Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyLoginToken()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                user.Settings = OverallUser.GenerateDefaultSettings();
                Assert.IsTrue(manipulator.UpdateUsersSettings(user));
                var currentSettings = user.Settings;
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructChangeSettingRequest(
                        user.UserId,
                        "x''",
                        loginTokens.AuthToken,
                        UserSettingsEntryKeys.DisplayName,
                        "New Name #2!"),
                    "PATCH");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(ctx);
                HttpWebResponse resp = null;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                using (resp)
                    Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyAuthToken()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                user.Settings = OverallUser.GenerateDefaultSettings();
                Assert.IsTrue(manipulator.UpdateUsersSettings(user));
                var currentSettings = user.Settings;
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructChangeSettingRequest(
                        user.UserId,
                        loginTokens.LoginToken,
                        "x''",
                        UserSettingsEntryKeys.DisplayName,
                        "New Name #2!"),
                    "PATCH");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(ctx);
                HttpWebResponse resp = null;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                using (resp)
                    Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyKey()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                user.Settings = OverallUser.GenerateDefaultSettings();
                Assert.IsTrue(manipulator.UpdateUsersSettings(user));
                var currentSettings = user.Settings;
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructChangeSettingRequest(
                        user.UserId,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        "",
                        "New Name #2!"),
                    "PATCH");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(ctx);
                HttpWebResponse resp = null;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                using (resp)
                    Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyValue()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                user.Settings = OverallUser.GenerateDefaultSettings();
                Assert.IsTrue(manipulator.UpdateUsersSettings(user));
                var currentSettings = user.Settings;
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructChangeSettingRequest(
                        user.UserId,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        UserSettingsEntryKeys.DisplayName,
                        ""),
                    "PATCH");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(ctx);
                HttpWebResponse resp = null;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                using (resp)
                    Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestUnauthorizedOnNonLoggedInUser()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                user.Settings = OverallUser.GenerateDefaultSettings();
                Assert.IsTrue(manipulator.UpdateUsersSettings(user));
                var currentSettings = user.Settings;
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructChangeSettingRequest(
                        user.UserId,
                        "x'ababaccabadba'",
                        loginTokens.AuthToken,
                        UserSettingsEntryKeys.DisplayName,
                        "New Name #2!"),
                    "PATCH");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(ctx);
                HttpWebResponse resp = null;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                using (resp)
                    Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestUnauthorizedOnNonAuthenticatedUser()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                user.Settings = OverallUser.GenerateDefaultSettings();
                Assert.IsTrue(manipulator.UpdateUsersSettings(user));
                var currentSettings = user.Settings;
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructChangeSettingRequest(
                        user.UserId,
                        loginTokens.LoginToken,
                        "x'ababccbadbacba'",
                        UserSettingsEntryKeys.DisplayName,
                        "New Name #2!"),
                    "PATCH");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(ctx);
                HttpWebResponse resp = null;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                using (resp)
                    Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentUser()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                user.Settings = OverallUser.GenerateDefaultSettings();
                Assert.IsTrue(manipulator.UpdateUsersSettings(user));
                var currentSettings = user.Settings;
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructChangeSettingRequest(
                        1000000,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        UserSettingsEntryKeys.DisplayName,
                        "New Name #2!"),
                    "PATCH");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(ctx);
                HttpWebResponse resp = null;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                using (resp)
                    Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentSetting()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                user.Settings = OverallUser.GenerateDefaultSettings();
                Assert.IsTrue(manipulator.UpdateUsersSettings(user));
                var currentSettings = user.Settings;
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(user);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructChangeSettingRequest(
                        user.UserId,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        "DisplayName",
                        "New Name #2!"),
                    "PATCH");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(ctx);
                HttpWebResponse resp = null;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                using (resp)
                    Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
            }
        }



    }
}

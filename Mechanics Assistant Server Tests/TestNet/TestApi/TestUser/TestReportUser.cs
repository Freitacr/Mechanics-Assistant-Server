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
    public class TestReportUser
    {
        private static ReportUserApi TestApi;

        [ClassInitialize]
        public static void InitializeClass(TestContext ctx)
        {
            if (!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initialize database schema. See logged error for details");
            TestApi = new ReportUserApi(10000);
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
        public void TestValidReport()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                var reportedUser = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser2.Email))[0];
                try
                {
                    reportedUser.UpdateSettings(UserSettingsEntryKeys.DisplayName, "TerribleName");
                    Assert.IsTrue(manipulator.UpdateUsersSettings(reportedUser));
                    NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                    var reportingUser = manipulator.GetUsersWhere(
                        string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                    var loginTokens = UserVerificationUtil.ExtractLoginTokens(reportingUser); 
                    object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                        TestingUserStorage.ValidUser1.ConstructReportMessage(
                            reportingUser.UserId,
                            loginTokens.LoginToken,
                            loginTokens.AuthToken,
                            "TerribleName"
                        ), "POST"
                    );

                    var ctx = contextAndRequest[0] as HttpListenerContext;
                    var req = contextAndRequest[1] as HttpWebRequest;
                    HttpWebResponse resp = null;
                    TestApi.POST(ctx);
                    try {
                        resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    } catch(WebException) {
                        Assert.Fail("Received an error message when one was not expected");
                    }

                    Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
                    reportedUser = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser2.Email))[0];
                    var reportedUserSettings = JsonDataObjectUtil<List<UserSettingsEntry>>.ParseObject(reportedUser.Settings);
                    bool foundDisplayName = false;
                    foreach(UserSettingsEntry entry in reportedUserSettings) {
                        if(entry.Key == UserSettingsEntryKeys.DisplayName)
                        {
                            foundDisplayName = true;
                            Assert.AreEqual("Default User " + reportedUser.UserId, entry.Value);
                            break;
                        }
                    }
                    Assert.IsTrue(foundDisplayName);
                } finally
                {
                    reportedUser.Settings = OverallUser.GenerateDefaultSettings();
                    Assert.IsTrue(manipulator.UpdateUsersSettings(reportedUser));
                }
            }
        }

        [TestMethod]
        public void TestBadRequestOnInvalidUserId()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var reportingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(reportingUser); 
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructReportMessage(
                        0,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        "TerribleName"
                    ), "POST"
                );

                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error message but never received one");
                } catch(WebException e) {
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
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var reportingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(reportingUser); 
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructReportMessage(
                        reportingUser.UserId,
                        "x''",
                        loginTokens.AuthToken,
                        "TerribleName"
                    ), "POST"
                );

                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error message but never received one");
                } catch(WebException e) {
                    resp = e.Response as HttpWebResponse;
                }

                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyAuthToken()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var reportingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(reportingUser); 
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructReportMessage(
                        reportingUser.UserId,
                        loginTokens.AuthToken,
                        "x''",
                        "TerribleName"
                    ), "POST"
                );

                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error message but never received one");
                } catch(WebException e) {
                    resp = e.Response as HttpWebResponse;
                }

                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyReportedName()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var reportingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(reportingUser); 
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructReportMessage(
                        reportingUser.UserId,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        ""
                    ), "POST"
                );

                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error message but never received one");
                } catch(WebException e) {
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
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var reportingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(reportingUser); 
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructReportMessage(
                        reportingUser.UserId,
                        "x'aabacabaadbaca'",
                        loginTokens.AuthToken,
                        "TerribleName"
                    ), "POST"
                );

                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error message but never received one");
                } catch(WebException e) {
                    resp = e.Response as HttpWebResponse;
                }

                Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestUnauthorizedOnNonAuthenticatedUser()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var reportingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(reportingUser); 
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructReportMessage(
                        reportingUser.UserId,
                        loginTokens.LoginToken,
                        loginTokens.LoginToken,
                        "TerribleName"
                    ), "POST"
                );

                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error message but never received one");
                } catch(WebException e) {
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
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var reportingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(reportingUser); 
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructReportMessage(
                        10000000,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        "TerribleName"
                    ), "POST"
                );

                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error message but never received one");
                } catch(WebException e) {
                    resp = e.Response as HttpWebResponse;
                }

                Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
            }
        }
    }
}

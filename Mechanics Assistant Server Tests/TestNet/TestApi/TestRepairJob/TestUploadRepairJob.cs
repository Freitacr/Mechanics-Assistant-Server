using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Net.Api;
using OldManInTheShopServer.Util;

namespace MechanicsAssistantServerTests.TestNet.TestApi.TestRepairJob {

    [TestClass]
    public class TestUploadRepairJob {
        
        private static RepairJobApi TestApi;

        [ClassInitialize]
        public static void SetupTests(TestContext context) {
            if(!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initalize database. See logged error");
            TestApi = new RepairJobApi(10000);
            if (!TestingDatabaseCreationUtils.InitializeUsers())
                throw new Exception("Failed to initialize users in database. See logged error");
        }

        [ClassCleanup]
        public static void CleanupTests() {
            ServerTestingMessageSwitchback.CloseSwitchback();
            if(!TestingDatabaseCreationUtils.DestoryDatabase())
                throw new Exception("Failed to destory database. This is bad. Manual deletion is required");
        }

        [TestMethod]
        public void TestUploadRepairJobNoSimilarJob(){
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1,manipulator);
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                        TestingRepairJobStorage.RepairJob1.ConstructCreationMessage(
                            uploadingUser.UserId,
                            loginTokens.LoginToken,
                            loginTokens.AuthToken,
                            0
                         ), "POST"
                     );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException)
                {
                    Assert.Fail("Received an error message when one was not expected");
                }
                Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
            }

        }

        [TestMethod]
        public void TestUploadRepairJobSimilarJobsForced() {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                        TestingRepairJobStorage.RepairJob1.ConstructCreationMessage(
                            uploadingUser.UserId,
                            loginTokens.LoginToken,
                            loginTokens.AuthToken,
                            1
                        ), "POST"
                    );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException)
                {
                    Assert.Fail("Received an error message when one was not expected");
                }
                Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
            }
        }

        [TestMethod]
        public void TestUploadRepairJobsSimilarJobsGetList() {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                
                manipulator.AddDataEntry(1,TestingRepairJobStorage.RepairJob1.CreateEntry(),true);
                manipulator.AddDataEntry(1, TestingRepairJobStorage.RepairJob2.CreateEntry(), false);

                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                        TestingRepairJobStorage.SimilarJob1.ConstructCreationMessage(
                            uploadingUser.UserId,
                            loginTokens.LoginToken,
                            loginTokens.AuthToken,
                            0
                        ), "POST"
                    );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error message but never received one");
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                    string message = e.Message;
                }
                Assert.AreEqual(HttpStatusCode.Conflict, resp.StatusCode);

            }
        }

        [TestMethod]
        public void TestBadRequestOnInvalidUserId() { 
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingRepairJobStorage.RepairJob1.ConstructCreationMessage(
                        -1,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        0
                    ), "POST"
                );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                    string message = e.Message;
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
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingRepairJobStorage.RepairJob1.ConstructCreationMessage(
                        uploadingUser.UserId,
                        "",
                        loginTokens.AuthToken,
                        0
                    ), "POST"
                );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                    string message = e.Message;
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
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingRepairJobStorage.RepairJob1.ConstructCreationMessage(
                        uploadingUser.UserId,
                        loginTokens.LoginToken,
                        "",
                        0
                    ), "POST"
                );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                    string message = e.Message;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }

        }

        [TestMethod]
        public void TestBadRequestOnEmptyRepairJobMake()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingRepairJobStorage.NoMakeJob.ConstructCreationMessage(
                        uploadingUser.UserId,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        0
                    ), "POST"
                );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                    string message = e.Message;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }

        }

        [TestMethod]
        public void TestBadRequestOnEmptyRepairJobModel()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingRepairJobStorage.NoModelJob.ConstructCreationMessage(
                        uploadingUser.UserId,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        0
                    ), "POST"
                );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                    string message = e.Message;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }

        }

        [TestMethod]
        public void TestBadRequestOnEmptyRepairJobProblem()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingRepairJobStorage.NoProblemJob.ConstructCreationMessage(
                        uploadingUser.UserId,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        0
                    ), "POST"
                );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                    string message = e.Message;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }

        }

        [TestMethod]
        public void TestBadRequestOnEmptyRepairJobComplaint()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingRepairJobStorage.NoComplaintJob.ConstructCreationMessage(
                        uploadingUser.UserId,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        0
                    ), "POST"
                );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                    string message = e.Message;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }

        }


        [TestMethod]
        public void TestBadRequestOnCrossSiteScriptingCharacters()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingRepairJobStorage.AttackJob.ConstructCreationMessage(
                        uploadingUser.UserId,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        0
                    ), "POST"
                );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                    string message = e.Message;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            }

        }

        [TestMethod]
        public void TestUnauthorizedOnNotLoggedInUser()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator);
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingRepairJobStorage.RepairJob1.ConstructCreationMessage(
                        uploadingUser.UserId,
                        "I'm Logged-In I Swear!",
                        loginTokens.AuthToken,
                        0
                    ), "POST"
                );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                    string message = e.Message;
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
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingRepairJobStorage.RepairJob1.ConstructCreationMessage(
                        uploadingUser.UserId,
                        loginTokens.LoginToken,
                        "I'm Autherized I Swear!",
                        0
                    ), "POST"
                );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                    string message = e.Message;
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
                var uploadingUser = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                    )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(uploadingUser);
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingRepairJobStorage.RepairJob1.ConstructCreationMessage(
                        uploadingUser.UserId+1000000,
                        loginTokens.LoginToken,
                        loginTokens.AuthToken,
                        0
                    ), "POST"
                );
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                HttpWebResponse resp = null;
                TestApi.POST(ctx);
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                    string message = e.Message;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
            }

        }

    }
}
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
    public class TestUserRetrievePastRequests
    {
        private static UserRequestsApi TestApi;
        
        [ClassInitialize]
        public static void InitializeClass(TestContext ctx)
        {
            if (!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initialize database schema. See logged error for details");
            TestApi = new UserRequestsApi(10000);
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
        public void TestRetrievePastRequests()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser1));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                
                List<PreviousUserRequest> currentRequests = user.DecodeRequests();
                currentRequests.Add(new PreviousUserRequest()
                {
                    Request = new RequestString() { Company = 1, Type = "TestingRequest" },
                    RequestStatus = "Completed"
                });
                user.EncodeRequests(currentRequests);
                Assert.IsTrue(manipulator.UpdateUserPreviousRequests(user));
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructRetrievePreviousRequestsRequest(
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
                    var receivedRequests = JsonDataObjectUtil<List<PreviousUserRequest>>.ParseObject(received);
                    Assert.AreEqual(currentRequests.Count, receivedRequests.Count);
                    for (int i = 0; i < currentRequests.Count; i++)
                        Assert.AreEqual(currentRequests[i], receivedRequests[i]);
                }
            }
        }

        [TestMethod]
        public void TestRetrievePastRequestsEmpty()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(NetTestingUserUtils.LogInTestingUser(TestingUserStorage.ValidUser1));
                var user = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email))[0];
                List<PreviousUserRequest> currentRequests = new List<PreviousUserRequest>();
                user.EncodeRequests(currentRequests);
                Assert.IsTrue(manipulator.UpdateUserPreviousRequests(user));
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructRetrievePreviousRequestsRequest(
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
                    var receivedRequests = JsonDataObjectUtil<List<PreviousUserRequest>>.ParseObject(received);
                    Assert.AreEqual(currentRequests.Count, receivedRequests.Count);
                    for (int i = 0; i < currentRequests.Count; i++)
                        Assert.AreEqual(currentRequests[i], receivedRequests[i]);
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
                List<PreviousUserRequest> currentRequests = user.DecodeRequests();
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructRetrievePreviousRequestsRequest(
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
                    Assert.Fail("Expected an error response, but did not receive one");
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
                List<PreviousUserRequest> currentRequests = user.DecodeRequests();
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructRetrievePreviousRequestsRequest(
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
                    Assert.Fail("Expected an error response, but did not receive one");
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
                List<PreviousUserRequest> currentRequests = user.DecodeRequests();
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructRetrievePreviousRequestsRequest(
                        user.UserId,
                        "x'ababbbaacbaba'"),
                    "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
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
                List<PreviousUserRequest> currentRequests = user.DecodeRequests();
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructRetrievePreviousRequestsRequest(
                        10000000,
                        UserVerificationUtil.ExtractLoginTokens(user).LoginToken),
                    "PUT");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PUT(ctx);
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
                Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
            }
        }

    }
}

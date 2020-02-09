using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManInTheShopServer.Net.Api;
using System.Net;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;

namespace MechanicsAssistantServerTests.TestNet.TestApi.TestUser
{
    [TestClass]
    public class TestUserCreationLocal
    {

        private static UserApi TestApi;

        [ClassInitialize]
        public static void SetupTests(TestContext ctx)
        {
            if (!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initialize database. See logged error.");
            TestApi = new UserApi(10000);
        }

        [ClassCleanup]
        public static void CleanupTests()
        {
            ServerTestingMessageSwitchback.CloseSwitchback();
            if (!TestingDatabaseCreationUtils.DestoryDatabase())
                throw new Exception("Failed to destroy testing database. This is bad. Manual cleanup is required");
        }

        [TestMethod]
        public void TestCreateValidUser1()
        {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            using (manipulator)
            {
                manipulator.Connect(TestingConstants.ConnectionString);
                Assert.IsTrue(manipulator.RemoveUserByEmail(TestingUserStorage.ValidUser1.Email));
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    TestingUserStorage.ValidUser1.ConstructCreationMessage(),
                    "POST");
                var ctx = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.POST(ctx);

                HttpWebResponse resp;
                try
                {
                    resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                }
                catch (WebException e)
                {
                    resp = e.Response as HttpWebResponse;
                }
                try
                {
                    Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
                }
                catch (AssertFailedException e)
                {
                    byte[] respData = new byte[resp.ContentLength];
                    resp.GetResponseStream().Read(respData, 0, respData.Length);
                    Console.WriteLine(Encoding.UTF8.GetString(respData));
                    TestingDatabaseCreationUtils.InitializeUsers();
                    throw e;
                }

                var createdUser = manipulator.GetUsersWhere(string.Format("Email = \"{0}\"", TestingUserStorage.ValidUser1.Email));
                Assert.IsNotNull(createdUser);
                Assert.AreEqual(1, createdUser.Count);
                Assert.AreEqual(TestingUserStorage.ValidUser1.Email, createdUser[0].Email);

            }
            
        }

        [TestMethod]
        public void TestConflictOnDuplicateUserCreation()
        {
            object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                TestingUserStorage.ValidUser1.ConstructCreationMessage(),
                "POST");
            var ctx = contextAndRequest[0] as HttpListenerContext;
            var req = contextAndRequest[1] as HttpWebRequest;

            HttpWebResponse resp;

            TestApi.POST(ctx);
            try
            {
                resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
            }
            catch(WebException e)
            {
                resp = e.Response as HttpWebResponse;
            }
            Assert.AreEqual(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [TestMethod]
        public void TestBadRequestOnEmptyEmail()
        {
            try
            {
                MySqlDataManipulator manipulator = new MySqlDataManipulator();
                using (manipulator)
                {
                    manipulator.Connect(TestingConstants.ConnectionString);
                    Assert.IsTrue(manipulator.RemoveUserByEmail(TestingUserStorage.ValidUser1.Email));
                    var creationMessage = TestingUserStorage.ValidUser1.ConstructCreationMessage();
                    creationMessage.SetMapping("Email", "");
                    object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                       creationMessage,
                       "POST");
                    var ctx = contextAndRequest[0] as HttpListenerContext;
                    var req = contextAndRequest[1] as HttpWebRequest;

                    HttpWebResponse resp;

                    TestApi.POST(ctx);
                    try
                    {
                        resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    }
                    catch (WebException e)
                    {
                        resp = e.Response as HttpWebResponse;
                    }
                    Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
                }
            }
            finally
            {
                TestingDatabaseCreationUtils.InitializeUsers();
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyPassword()
        {
            try
            {
                MySqlDataManipulator manipulator = new MySqlDataManipulator();
                using (manipulator)
                {
                    manipulator.Connect(TestingConstants.ConnectionString);
                    Assert.IsTrue(manipulator.RemoveUserByEmail(TestingUserStorage.ValidUser1.Email));
                    var creationMessage = TestingUserStorage.ValidUser1.ConstructCreationMessage();
                    creationMessage.SetMapping("Password", "");
                    object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                       creationMessage,
                       "POST");
                    var ctx = contextAndRequest[0] as HttpListenerContext;
                    var req = contextAndRequest[1] as HttpWebRequest;

                    HttpWebResponse resp;

                    TestApi.POST(ctx);
                    try
                    {
                        resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    }
                    catch (WebException e)
                    {
                        resp = e.Response as HttpWebResponse;
                    }
                    Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
                }
            } finally {
                TestingDatabaseCreationUtils.InitializeUsers();
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptySecurityQuestion()
        {
            try
            {
                MySqlDataManipulator manipulator = new MySqlDataManipulator();
                using (manipulator)
                {
                    manipulator.Connect(TestingConstants.ConnectionString);
                    Assert.IsTrue(manipulator.RemoveUserByEmail(TestingUserStorage.ValidUser1.Email));
                    var creationMessage = TestingUserStorage.ValidUser1.ConstructCreationMessage();
                    creationMessage.SetMapping("SecurityQuestion", "");
                    object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                       creationMessage,
                       "POST");
                    var ctx = contextAndRequest[0] as HttpListenerContext;
                    var req = contextAndRequest[1] as HttpWebRequest;

                    HttpWebResponse resp;

                    TestApi.POST(ctx);
                    try
                    {
                        resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    }
                    catch (WebException e)
                    {
                        resp = e.Response as HttpWebResponse;
                    }
                    Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
                }
            } finally
            {
                TestingDatabaseCreationUtils.InitializeUsers();
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptySecurityAnswer()
        {
            try
            {
                MySqlDataManipulator manipulator = new MySqlDataManipulator();
                using (manipulator)
                {
                    manipulator.Connect(TestingConstants.ConnectionString);
                    Assert.IsTrue(manipulator.RemoveUserByEmail(TestingUserStorage.ValidUser1.Email));
                    var creationMessage = TestingUserStorage.ValidUser1.ConstructCreationMessage();
                    creationMessage.SetMapping("SecurityAnswer", "");
                    object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                       creationMessage,
                       "POST");
                    var ctx = contextAndRequest[0] as HttpListenerContext;
                    var req = contextAndRequest[1] as HttpWebRequest;

                    HttpWebResponse resp;

                    TestApi.POST(ctx);
                    try
                    {
                        resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    }
                    catch (WebException e)
                    {
                        resp = e.Response as HttpWebResponse;
                    }
                    Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
                }
            } finally
            {
                TestingDatabaseCreationUtils.InitializeUsers();
            }
        }

        [TestMethod]
        public void TestBadRequestOnAllEmptyFields()
        {
            try
            {
                MySqlDataManipulator manipulator = new MySqlDataManipulator();
                using (manipulator)
                {
                    manipulator.Connect(TestingConstants.ConnectionString);
                    Assert.IsTrue(manipulator.RemoveUserByEmail(TestingUserStorage.ValidUser1.Email));
                    var creationMessage = new JsonDictionaryStringConstructor();
                    object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                       creationMessage,
                       "POST");
                    var ctx = contextAndRequest[0] as HttpListenerContext;
                    var req = contextAndRequest[1] as HttpWebRequest;

                    HttpWebResponse resp;

                    TestApi.POST(ctx);
                    try
                    {
                        resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    }
                    catch (WebException e)
                    {
                        resp = e.Response as HttpWebResponse;
                    }
                    Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
                }
            }
            finally
            {
                TestingDatabaseCreationUtils.InitializeUsers();
            }
        }
    }
}

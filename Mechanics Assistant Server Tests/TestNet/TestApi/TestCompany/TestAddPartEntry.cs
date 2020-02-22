using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Net.Api;
using OldManInTheShopServer.Util;

namespace MechanicsAssistantServerTests.TestNet.TestApi.TestCompany {

    [TestClass]
    public class TestAddPartEntry
    {
        private static CompanyPartsApi TestApi;

        [ClassInitialize]
        public static void InitializeClass(TestContext context) {
            if (!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initialize database schema. See logged error for details");
            TestApi = new CompanyPartsApi(10000);
        }

        [ClassCleanup]
        public static void CleanupTests() {
            ServerTestingMessageSwitchback.CloseSwitchback();
            if(!TestingDatabaseCreationUtils.DestoryDatabase())
                throw new Exception("Failed to destroy testing database. This is bad. Manual deletion required");
        }

        [TestMethod]
        public void TestValidRequest() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                PartCatalogueEntry entry = manipulator.GetPartCatalogueEntriesWhere(1, 
                    string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                    )[0];
                Assert.IsTrue(manipulator.RemovePartCatalogueEntry(
                    1, entry.Id
                ));
                var entryList = manipulator.GetPartCatalogueEntriesWhere(
                        1, 
                        string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                    );
                Assert.AreEqual(0, entryList.Count);
                try {
                    Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                    OverallUser validUser1 = manipulator.GetUsersWhere(
                        string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                    )[0];
                    var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                    var message = TestingPartEntry.ValidPartEntry1.ConstructAdditionRequest(
                        validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken
                    );
                    object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                        message, "POST"
                    );
                    var context = contextAndRequest[0] as HttpListenerContext;
                    var req = contextAndRequest[1] as HttpWebRequest;
                    TestApi.POST(context);
                    HttpWebResponse response;
                    try {
                        response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    } catch (WebException e) {
                        response = e.Response as HttpWebResponse;
                        Assert.Fail("Server sent back an error response: {0}", 
                            response.StatusCode);
                    }
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                    var addedEntryList = manipulator.GetPartCatalogueEntriesWhere(
                        1, 
                        string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                    );
                    Assert.AreEqual(1, addedEntryList.Count);
                } finally {
                    if(
                        manipulator.GetPartCatalogueEntriesWhere(1, 
                            string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                        ).Count == 0
                    ) {
                        Assert.IsTrue(TestingDatabaseCreationUtils.InitializePartCatelogueEntries());
                    }
                }
            }
        }

        [TestMethod]
        public void TestBadRequestOnInvalidUserId() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructAdditionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken
                );
                message["UserId"] = 0;
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "POST"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.POST(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyLoginToken() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructAdditionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken
                );
                message["LoginToken"] = "x''";
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "POST"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.POST(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyAuthToken() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructAdditionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken
                );
                message["AuthToken"] = "x''";
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "POST"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.POST(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyMake() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructAdditionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken
                );
                message["Make"] = "";
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "POST"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.POST(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyModel() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructAdditionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken
                );
                message["Model"] = "";
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "POST"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.POST(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnInvalidYear() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructAdditionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken
                );
                message["Year"] = -1;
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "POST"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.POST(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyPartId() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructAdditionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken
                );
                message["PartId"] = "";
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "POST"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.POST(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnEmptyPartName() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructAdditionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken
                );
                message["PartName"] = "";
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "POST"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.POST(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestUnauthorizedOnNonPartsUser() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructAdditionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken
                );
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "POST"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.POST(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestUnauthorizedOnNonAuthenticatedUser() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructAdditionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken
                );
                message["AuthToken"] = "x'abaccabadabaca'";
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "POST"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.POST(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestUnauthorizedOnNonLoggedInUser() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructAdditionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken
                );
                message["LoginToken"] = "x'abacbabccba'";
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "POST"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.POST(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentUser() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructAdditionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken
                );
                message["UserId"] = 100000;
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "POST"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.POST(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected an error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            }
        }
    }


}
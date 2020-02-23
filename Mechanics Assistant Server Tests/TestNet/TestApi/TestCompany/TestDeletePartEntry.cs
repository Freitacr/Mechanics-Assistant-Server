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
    public class TestDeletePartEntry
    {
        private static CompanyPartsApi TestApi;

        [ClassInitialize]
        public static void InitializeClass(TestContext context) {
            if (!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initialize database schema. See logged error for details");
            TestApi = new CompanyPartsApi(10000);
            if (!TestingDatabaseCreationUtils.InitializeUsers())
                throw new Exception("Failed to initialize users in database. See logged error for details");
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
                try {
                    Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                    OverallUser validUser1 = manipulator.GetUsersWhere(
                        string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                    )[0];
                    var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                    var message = TestingPartEntry.ValidPartEntry1.ConstructDeletionRequest(
                        validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken, entry.Id
                    );
                    object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                        message, "PATCH"
                    );
                    var context = contextAndRequest[0] as HttpListenerContext;
                    var req = contextAndRequest[1] as HttpWebRequest;
                    TestApi.PATCH(context);
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
                    Assert.AreEqual(0, addedEntryList.Count);
                } finally {
                    if(

                        manipulator.GetPartCatalogueEntriesWhere(1, 
                            string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                        ).Count == 0
                    ) {
                        Assert.IsTrue(manipulator.AddPartEntry(1, entry));
                    } else {
                        Assert.Fail("Removal of part entry failed");
                    }
                }
            }
        }

        [TestMethod]
        public void TestBadRequestOnInvalidUserId() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                PartCatalogueEntry entry = manipulator.GetPartCatalogueEntriesWhere(1, 
                    string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                    )[0];
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructDeletionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken, entry.Id
                );
                message["UserId"] = 0;
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "PATCH"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected and error response, but did not receive one");
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
                PartCatalogueEntry entry = manipulator.GetPartCatalogueEntriesWhere(1, 
                    string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                    )[0];
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructDeletionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken, entry.Id
                );
                message["LoginToken"] = "x''";
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "PATCH"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected and error response, but did not receive one");
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
                PartCatalogueEntry entry = manipulator.GetPartCatalogueEntriesWhere(1, 
                    string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                    )[0];
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructDeletionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken, entry.Id
                );
                message["AuthToken"] = "x''";
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "PATCH"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected and error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestBadRequestOnInvalidPartEntryId() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                PartCatalogueEntry entry = manipulator.GetPartCatalogueEntriesWhere(1, 
                    string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                    )[0];
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructDeletionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken, entry.Id
                );
                message["PartEntryId"] = 0;
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "PATCH"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected and error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentPartEntry() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                PartCatalogueEntry entry = manipulator.GetPartCatalogueEntriesWhere(1, 
                    string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                    )[0];
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructDeletionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken, entry.Id
                );
                message["PartEntryId"] = 40;
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "PATCH"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected and error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [TestMethod]
        public void TestUnauthorizedOnNonPartsUser() {
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            Assert.IsTrue(manipulator.Connect(TestingConstants.ConnectionString));
            using(manipulator) {
                PartCatalogueEntry entry = manipulator.GetPartCatalogueEntriesWhere(1, 
                    string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                    )[0];
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser1, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructDeletionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken, entry.Id
                );
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "PATCH"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected and error response, but did not receive one");
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
                PartCatalogueEntry entry = manipulator.GetPartCatalogueEntriesWhere(1, 
                    string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                    )[0];
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructDeletionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken, entry.Id
                );
                message["AuthToken"] = "x'aabacabab'";
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "PATCH"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected and error response, but did not receive one");
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
                PartCatalogueEntry entry = manipulator.GetPartCatalogueEntriesWhere(1, 
                    string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                    )[0];
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructDeletionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken, entry.Id
                );
                message["LoginToken"] = "x'abacbadabac'";
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "PATCH"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected and error response, but did not receive one");
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
                PartCatalogueEntry entry = manipulator.GetPartCatalogueEntriesWhere(1, 
                    string.Format("PartId=\"{0}\"", TestingPartEntry.ValidPartEntry1.PartId)
                    )[0];
                Assert.IsTrue(NetTestingUserUtils.AuthenticateTestingUser(TestingUserStorage.ValidUser3, manipulator));
                OverallUser validUser1 = manipulator.GetUsersWhere(
                    string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)
                )[0];
                var loginTokens = UserVerificationUtil.ExtractLoginTokens(validUser1);
                var message = TestingPartEntry.ValidPartEntry1.ConstructDeletionRequest(
                    validUser1.UserId, loginTokens.LoginToken, loginTokens.AuthToken, entry.Id
                );
                message["UserId"] = 10000;
                object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                    message, "PATCH"
                );
                var context = contextAndRequest[0] as HttpListenerContext;
                var req = contextAndRequest[1] as HttpWebRequest;
                TestApi.PATCH(context);
                HttpWebResponse response;
                try {
                    response = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                    Assert.Fail("Expected and error response, but did not receive one");
                } catch (WebException e) {
                    response = e.Response as HttpWebResponse;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            }
        }
    }


}
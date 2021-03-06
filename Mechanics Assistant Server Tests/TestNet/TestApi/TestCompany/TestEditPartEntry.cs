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
    public class TestEditPartEntry
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
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnInvalidUserId() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnEmptyLoginToken() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnEmptyAuthToken() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnEmptyFieldName() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnEmptyFieldValue() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnEmptyPartEntryId() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentFieldName() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentPartEntry() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestUnauthorizedOnNonPartsUser() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestUnauthorizedOnNonAuthenticatedUser() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestUnauthorizedOnNonLoggedInUser() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentUser() {
            throw new NotImplementedException();
        }
    }


}
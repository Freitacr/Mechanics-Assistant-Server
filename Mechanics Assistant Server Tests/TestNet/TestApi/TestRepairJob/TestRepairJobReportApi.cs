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
    public class TestRepairJobReportApi {
        
        private static RepairJobReportApi TestApi;

        [ClassInitialize]
        public static void SetupTests(TestContext context) {
            if(!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initalize database. See logged error");
            TestApi = new RepairJobReportApi(10000);
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
        public void TestValidReport() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnInvalidUserId() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnInvalidLoginToken() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnInvalidAuthToken() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnInvalidRepairJobId() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnInvalidUpvoteValue() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestUnauthorizedOnNonLoggedInUser() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestUnauthorizedOnNonAuthenticatedUser(){
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentUser() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentRepairJob() {
            throw new NotImplementedException();
        }

    }
}
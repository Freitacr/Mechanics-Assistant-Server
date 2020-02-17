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
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestUploadRepairJobSimilarJobsForced() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestUploadRepairJobsSimilarJobsGetList() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestConflictOnUploadRepairJobDuplicateJobForced(){
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnInvalidUserId() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnEmptyLoginToken(){
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnEmptyAuthToken() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnEmptyRepairJobMake() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnEmptyRepairJobModel(){
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnEmptyRepairJobProblem() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnEmptyRepairJobComplaint() {
            throw new NotImplementedException();
        }


        [TestMethod]
        public void TestBadRequestOnCrossSiteScriptingCharacters() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnInvalidDuplicateValue() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestUnauthorizedOnNotLoggedInUser() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestUnauthorizedOnNonAuthenticatedUser() {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentUser() {
            throw new NotImplementedException();
        }   



    }
}
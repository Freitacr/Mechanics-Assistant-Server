using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Net.Api;
using OldManInTheShopServer.Util;

namespace MechanicsAssistantServerTests.TestNet.TestApi.TestCompany
{
    [TestClass]
    public class TestUpdateUserAccessLevel
    {

        private static CompanyUsersApi TestApi;

        [ClassInitialize]
        public static void InitializeClass(TestContext ctx)
        {
            if (!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initialize database schema. See Logged error for details");
            TestApi = new CompanyUsersApi(10000);
        }

        [ClassCleanup]
        public static void CleanupTests()
        {
            ServerTestingMessageSwitchback.CloseSwitchback();
            if (!TestingDatabaseCreationUtils.DestoryDatabase())
                throw new Exception("Failed to destroy testing database. This is bad. Manual deletion is required");
        }


        [TestMethod]
        public void TestValidRequest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnInvalidUserId()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnEmptyLoginToken()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnEmptyAuthToken()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnInvalidCompanyUserId()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestBadRequestOnInvalidAccessLevel()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestNotFoundOnNonExistentUser()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestNotFoundOnNonExistingUserToChange()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestUnauthorizedOnNotLoggedInUser()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestUnauthorizedOnNonAuthorizedUser()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestUnauthorizedOnNonAdministrativeUser()
        {
            throw new NotImplementedException();
        }
    }
}

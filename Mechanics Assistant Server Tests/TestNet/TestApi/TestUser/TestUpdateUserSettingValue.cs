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
    class TestUpdateUserSettingValue
    {

        private static UserSettingsApi TestApi;

        [ClassInitialize]
        public static void InitializeClass(TestContext ctx)
        {
            if (!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initialize database schema. See logged error for details");
            TestApi = new UserSettingsApi(10000);
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
        public void TestUpdateUserSetting()
        {

        }

        [TestMethod]
        public void TestBadRequestOnInvalidUserId()
        {

        }

        [TestMethod]
        public void TestBadRequestOnEmptyLoginToken()
        {

        }

        [TestMethod]
        public void TestBadRequestOnEmptyAuthToken()
        {

        }

        [TestMethod]
        public void TestBadRequestOnEmptyKey()
        {

        }

        [TestMethod]
        public void TestBadRequestOnEmptyValue()
        {

        }

        [TestMethod]
        public void TestUnauthorizedOnNonLoggedInUser()
        {

        }

        [TestMethod]
        public void TestUnauthorizedOnNonAuthenticatedUser()
        {

        }

        [TestMethod]
        public void TestNotFoundOnNonExistentUser()
        {

        }

        [TestMethod]
        public void TestNotFoundOnNonExistentSetting()
        {

        }



    }
}

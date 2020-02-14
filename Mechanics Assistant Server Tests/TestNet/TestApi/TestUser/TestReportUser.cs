﻿using System;
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
    public class TestReportUser
    {
        private static ReportUserApi TestApi;

        [ClassInitialize]
        public static void InitializeClass(TestContext ctx)
        {
            if (!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initialize database schema. See logged error for details");
            TestApi = new ReportUserApi(10000);
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
        public void TestValidReport()
        {
            using (MySqlDataManipulator manipulator = new MySqlDataManipulator())
            {
                var reportedUser = manipulator.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser2.Email))[0];
                try
                {
                       
                } finally
                {
                    reportedUser.Settings = OverallUser.GenerateDefaultSettings();
                    Assert.IsTrue(manipulator.UpdateUsersSettings(reportedUser));
                }
            }
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
        public void TestBadRequestOnEmptyReportedName()
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
    }
}

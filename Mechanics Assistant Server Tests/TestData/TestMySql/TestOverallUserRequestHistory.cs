using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using MySql.Data.MySqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Net;
using OldManInTheShopServer.Net.Api;
using OldManInTheShopServer.Util;
using OldManInTheShopServer.Data.MySql.TableDataTypes;

namespace MechanicsAssistantServerTests.TestData.TestMySql
{
    [TestClass]
    public class TestOverallUserRequestHistory
    {

        private static MySqlDataManipulator Manipulator;
        private static readonly string ConnectionString = new MySqlConnectionString("localhost", "db_test", "testUser").ConstructConnectionString("");
        

        [ClassInitialize]
        public static void SetupTestSuite(TestContext ctx)
        {
            if(!TestingDatabaseCreationUtils.InitializeDatabaseSchema())
                throw new Exception("Failed to initialize database schema. See logged error");
            Manipulator = new MySqlDataManipulator();
            bool res = Manipulator.Connect(TestingConstants.ConnectionString);
            Manipulator.AddUser("msn", "1234", "red", "blue");
        }

        [ClassCleanup]
        public static void CleanupTestSuite()
        {
            if(!TestingDatabaseCreationUtils.DestoryDatabase())
                throw new Exception("Failed to destroy database. This is bad. Manual deletion required");
            Manipulator.Close();
        }

        [TestMethod]
        public void TestAddToUserRequestHistoryFromManipulator()
        {
            var user = Manipulator.GetUsersWhere("Email=\"msn\"")[0];
            List<PreviousUserRequest> emptyRequests = new List<PreviousUserRequest>();
            Assert.AreEqual(0, emptyRequests.Count);
            List<PreviousUserRequest> appendedTo = new List<PreviousUserRequest>();
            appendedTo.Add(new PreviousUserRequest() { Request = new RequestString() { Company = 1, Type = "Join" } });
            appendedTo[0].Request.CalculateMD5();
            user.EncodeRequests(appendedTo);
            byte[] requestHistory = user.RequestHistory;
            user.DecodeRequests();
            Manipulator.UpdateUserPreviousRequests(user);
            user = Manipulator.GetUsersWhere("Email=\"msn\"")[0];
            Assert.AreEqual(requestHistory.Length, user.RequestHistory.Length);
            for(int i = 0; i < requestHistory.Length; i++)
                Assert.AreEqual(requestHistory[i], user.RequestHistory[i]);
            emptyRequests = user.DecodeRequests();
            Assert.AreEqual(1, emptyRequests.Count);
        }
    }
}

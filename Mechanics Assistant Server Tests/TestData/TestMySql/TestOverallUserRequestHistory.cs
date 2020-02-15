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
            Manipulator = new MySqlDataManipulator();
            MySqlDataManipulator.GlobalConfiguration.Connect(ConnectionString);
            MySqlDataManipulator.GlobalConfiguration.Close();
            bool res = Manipulator.Connect(ConnectionString);
            if (!Manipulator.ValidateDatabaseIntegrity(TestingConstants.DatabaselessConnectionString, "db_test"))
            {
                Console.WriteLine("Encountered an error opening the global configuration connection");
                Console.WriteLine(MySqlDataManipulator.GlobalConfiguration.LastException.Message);
                return;
            }
            if (!res)
            {
                if (!Manipulator.Connect(ConnectionString))
                {
                    Console.WriteLine("Encountered an error opening the global configuration connection");
                    Console.WriteLine(MySqlDataManipulator.GlobalConfiguration.LastException.Message);
                    return;
                }
            }
            Manipulator.AddUser("msn", "1234", "red", "blue");
        }

        [ClassCleanup]
        public static void CleanupTestSuite()
        {
            MySqlConnection connection = new MySqlConnection();
            connection.ConnectionString = ConnectionString;
            connection.Open();
            using (connection)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "drop schema db_test;";
                cmd.ExecuteNonQuery();
            }
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

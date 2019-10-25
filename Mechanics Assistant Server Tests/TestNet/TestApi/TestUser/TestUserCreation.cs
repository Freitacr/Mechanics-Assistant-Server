using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Net;
using OldManInTheShopServer.Net.Api;
using OldManInTheShopServer.Util;

namespace MechanicsAssistantServerTests.TestNet.TestApi.TestUser
{
    [TestClass]
    public class TestUserCreation
    {
        private static HttpClient Client;
        private static MySqlDataManipulator Manipulator;
        private static QueryResponseServer Server;
        private static readonly string ConnectionString = new MySqlConnectionString("localhost", "db_test", "testUser").ConstructConnectionString("");
        private static readonly JsonDictionaryStringConstructor StringConstructor = new JsonDictionaryStringConstructor();

        [ClassInitialize]
        public static void SetupTestSuite(TestContext ctx)
        {
            Client = new HttpClient();
            Manipulator = new MySqlDataManipulator();
            MySqlDataManipulator.GlobalConfiguration.Connect(ConnectionString);
            MySqlDataManipulator.GlobalConfiguration.Close();
            bool res = Manipulator.Connect(ConnectionString);
            if (!Manipulator.ValidateDatabaseIntegrity("db_test"))
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
            Server = ApiLoader.LoadApiAndListen(16384);
        }

        [TestInitialize]
        public void FillStringConstructor()
        {
            StringConstructor.SetMapping("Email", "abcd@msn");
            StringConstructor.SetMapping("Password", 12345);
            StringConstructor.SetMapping("SecurityQuestion", "What is your favourite colour?");
            StringConstructor.SetMapping("SecurityAnswer", "Red");
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
            Server.Close();
            Manipulator.Close();
        }

        [TestMethod]
        public void TestAddUserEmptyEmail()
        {
            StringConstructor.SetMapping("Email", "");
            string testString = StringConstructor.ToString();
            StringContent postData = new StringContent(testString);
            var response = Client.PostAsync("http://localhost:16384/user", postData);
            var actualResponse = response.Result;
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, actualResponse.StatusCode);
            Assert.AreEqual("Not all fields of the request were filled", actualResponse.Content.ReadAsStringAsync().Result);
        }

        [TestMethod]
        public void TestAddUserIncorrectFormat()
        {
            StringConstructor.RemoveMapping("Email");
            string testString = StringConstructor.ToString();
            StringContent postData = new StringContent(testString);
            var response = Client.PostAsync("http://localhost:16384/user", postData);
            var actualResponse = response.Result;
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, actualResponse.StatusCode);
            Assert.AreEqual("Incorrect Format", actualResponse.ReasonPhrase);
        }

        [TestMethod]
        public void TestAdduserProperFormat()
        {
            MySqlConnection connection = new MySqlConnection();
            connection.ConnectionString = ConnectionString;
            connection.Open();
            using (connection)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "select max(id) from " + TableNameStorage.OverallUserTable;
                var reader = cmd.ExecuteReader();
                reader.Read();
                int prevId;

                using (reader)
                    prevId = reader.IsDBNull(0) ? 0 : (int)reader[0];

                string testString = StringConstructor.ToString();
                StringContent postData = new StringContent(testString);
                var response = Client.PostAsync("http://localhost:16384/user", postData);
                var actualResponse = response.Result;
                Assert.AreEqual(System.Net.HttpStatusCode.OK, actualResponse.StatusCode);

                
                reader = cmd.ExecuteReader();
                reader.Read();
                using (reader)
                {
                    Assert.IsFalse(reader.IsDBNull(0));
                    var id = (int)reader[0];
                    Assert.AreEqual(prevId + 1, id);
                    var user = Manipulator.GetUserById(id);
                    Assert.IsFalse(user == null);
                    Assert.AreEqual("abcd@msn", user.Email);
                    Assert.AreEqual("What is your favourite colour?", user.SecurityQuestion);
                }
            }
        }
    }
}

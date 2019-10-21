using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using MySql.Data.MySqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MechanicsAssistantServer.Data.MySql;
using MechanicsAssistantServer.Net;
using MechanicsAssistantServer.Net.Api;
using MechanicsAssistantServer.Util;

namespace MechanicsAssistantServerTests.TestNet.TestApi
{
    [TestClass]
    public class TestUserAuthGetAuthToken
    {
        private static HttpClient Client;
        private static MySqlDataManipulator Manipulator;
        private static QueryResponseServer Server;
        private static readonly string ConnectionString = new MySqlConnectionString("localhost", "db_test", "testUser").ConstructConnectionString("");
        private static string LoginToken;
        private static readonly string SecurityQuestion = "What is your favourite colour?";

        [ClassInitialize]
        public static void SetupTestSuite(TestContext ctx)
        {
            Client = new HttpClient();
            Manipulator = new MySqlDataManipulator();
            MySqlDataManipulator.GlobalConfiguration.Connect(ConnectionString);
            MySqlDataManipulator.GlobalConfiguration.Close();
            bool res = Manipulator.Connect(ConnectionString);
            if (res)
            {
                MySqlConnection connection = new MySqlConnection()
                {
                    ConnectionString = ConnectionString
                };
                connection.Open();
                using (connection)
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "drop schema db_test;";
                    cmd.ExecuteNonQuery();
                }
            }
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
            Manipulator.AddUser("abcd@msn", "12345", SecurityQuestion, "red");
            var content = new StringContent("{\"Email\":\"abcd@msn\",\"Password\":12345}");
            var response = Client.PutAsync("http://localhost:16384/user", content).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                Console.WriteLine("Test will fail due to sql error:" + response.Content.ReadAsStringAsync().Result);
            }
            Assert.IsTrue(response.IsSuccessStatusCode);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ExpectedLoginResponse));
            var responseContent = (ExpectedLoginResponse)serializer.ReadObject(response.Content.ReadAsStreamAsync().Result);
            LoginToken = responseContent.Token;
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
        public void TestGetAuthTokenEmptyLoginToken()
        {
            string testString = "{\"UserId\":1, \"LoginToken\":\"\", \"SecurityQuestion\":\""+SecurityQuestion+"\",\"SecurityAnswer\":\"red\"}";
            StringContent putData = new StringContent(testString);
            var response = Client.PutAsync("http://localhost:16384/user/auth", putData);
            var actualResponse = response.Result;
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, actualResponse.StatusCode);
            Assert.AreEqual("Not all fields of the request were filled", actualResponse.Content.ReadAsStringAsync().Result);
        }

        [TestMethod]
        public void TestGetAuthTokenIncorrectFormat()
        {
            string testString = "{\"UserId\":1, \"LoginToken\":\"\", \"Password\":\"" + SecurityQuestion + "\",\"SecurityAnswer\":\"red\"}"; 
            StringContent putData = new StringContent(testString);
            var response = Client.PutAsync("http://localhost:16384/user/auth", putData);
            var actualResponse = response.Result;
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, actualResponse.StatusCode);
            Assert.AreEqual("Incorrect Format", actualResponse.ReasonPhrase);
        }

        [TestMethod]
        public void TestGetAuthTokenNonExistantUser()
        {
            string testString = "{\"UserId\":3, \"LoginToken\":\"0xbaaaad\", \"SecurityQuestion\":\"" + SecurityQuestion + "\",\"SecurityAnswer\":\"red\"}"; 
            StringContent putData = new StringContent(testString);
            var response = Client.PutAsync("http://localhost:16384/user/auth", putData);
            var actualResponse = response.Result;
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, actualResponse.StatusCode);
        }

        [TestMethod]
        public void TestGetAuthTokenBadLoginToken()
        {
            string testString = "{\"UserId\":1, \"LoginToken\":\"0xbaaaad\", \"SecurityQuestion\":\"" + SecurityQuestion + "\",\"SecurityAnswer\":\"red\"}"; 
            StringContent putData = new StringContent(testString);
            var response = Client.PutAsync("http://localhost:16384/user/auth", putData);
            var actualResponse = response.Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, actualResponse.StatusCode);
        }

        [TestMethod]
        public void TestGetAuthTokenBadAnswer()
        {
            string testString = "{\"UserId\":1, \"LoginToken\":\""+LoginToken+"\", \"SecurityQuestion\":\"" + SecurityQuestion + "\",\"SecurityAnswer\":\"blue\"}";
            StringContent putData = new StringContent(testString);
            var response = Client.PutAsync("http://localhost:16384/user/auth", putData);
            var actualResponse = response.Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, actualResponse.StatusCode);
        }

        [TestMethod]
        public void TestGetAuthTokenProperFormat()
        {
            string testString = "{\"UserId\":1, \"LoginToken\":\""+LoginToken+"\", \"SecurityQuestion\":\"" + SecurityQuestion + "\",\"SecurityAnswer\":\"red\"}"; 
            StringContent putData = new StringContent(testString);
            var response = Client.PutAsync("http://localhost:16384/user/auth", putData);
            var actualResponse = response.Result;
            Assert.IsTrue(actualResponse.IsSuccessStatusCode);
            var respString = actualResponse.Content.ReadAsStringAsync().Result;
            Assert.IsTrue(UserVerificationUtil.AuthTokenValid(Manipulator.GetUserById(1), respString));
        }
    }
}

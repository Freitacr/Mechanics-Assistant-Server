using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using MySql.Data.MySqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Net;
using OldManInTheShopServer.Net.Api;
using System.IO;
using OldManInTheShopServer.Util;

namespace MechanicsAssistantServerTests.TestNet.TestApi.TestUser
{
    [TestClass]
    public class TestReportUser
    {
        private static HttpClient Client;
        private static MySqlDataManipulator Manipulator;
        private static QueryResponseServer Server;
        private static readonly string ConnectionString = new MySqlConnectionString("localhost", "db_test", "testUser").ConstructConnectionString("");
        private static string LoginToken;
        private static string AuthToken;
        private static readonly string SecurityQuestion = "What is your favourite colour?";
        private static readonly string Uri = "http://localhost:16384/user/report";
        private static readonly JsonStringConstructor JsonStringConstructor = new JsonStringConstructor();

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
                Console.WriteLine("Test will fail due to error:" + response.Content.ReadAsStringAsync().Result);
            }
            Assert.IsTrue(response.IsSuccessStatusCode);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ExpectedLoginResponse));
            var responseContent = (ExpectedLoginResponse)serializer.ReadObject(response.Content.ReadAsStreamAsync().Result);
            LoginToken = responseContent.Token;

            content = new StringContent("{\"UserId\":" + responseContent.Id + ",\"LoginToken\":\"" + responseContent.Token + "\",\"SecurityQuestion\":\"" + SecurityQuestion + "\",\"SecurityAnswer\":\"red\"}");
            response = Client.PutAsync("http://localhost:16384/user/auth", content).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                Console.WriteLine("Test will fail due to error:" + response.Content.ReadAsStringAsync().Result);
            }
            Assert.IsTrue(response.IsSuccessStatusCode);
            AuthToken = response.Content.ReadAsStringAsync().Result;
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

        [TestInitialize]
        public void FillStringConstructor()
        {
            JsonStringConstructor.SetMapping("DisplayName", "defaultUser");
            JsonStringConstructor.SetMapping("UserId", 1);
            JsonStringConstructor.SetMapping("AuthToken", AuthToken);
            JsonStringConstructor.SetMapping("LoginToken", LoginToken);
        }

        [TestMethod]
        public void TestReportUserIncorrectFormat()
        {
            JsonStringConstructor.RemoveMapping("DisplayName");
            string testString = JsonStringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void TestReportUserEmptyDisplayName()
        {
            JsonStringConstructor.SetMapping("DisplayName", "");
            string testString = JsonStringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void TestReportUserUnknownUser()
        {
            JsonStringConstructor.SetMapping("UserId", 3);
            string testString = JsonStringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestReportUserInvalidLoginToken()
        {
            JsonStringConstructor.SetMapping("LoginToken", "0xbaaaad");
            string testString = JsonStringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestReportUserInvalidAuthToken()
        {
            JsonStringConstructor.SetMapping("AuthToken", "0xbaaaad");
            string testString = JsonStringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestReportUserValidRequest()
        {
            string testString = JsonStringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            
            var user = Manipulator.GetUserById(1);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<SettingsEntry>));
            MemoryStream streamIn = new MemoryStream(Encoding.UTF8.GetBytes(user.Settings));
            var settings = serializer.ReadObject(streamIn) as List<SettingsEntry>;
            foreach (SettingsEntry entry in settings)
            {
                if (entry.Key.Equals("displayName"))
                {
                    Assert.AreEqual("Default User 1", entry.Value);
                    break;
                }
            }
        }
    }
}

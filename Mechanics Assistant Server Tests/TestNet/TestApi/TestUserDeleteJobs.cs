using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using MySql.Data.MySqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManinTheShopServer.Data.MySql;
using OldManinTheShopServer.Data.MySql.TableDataTypes;
using OldManinTheShopServer.Net;
using OldManinTheShopServer.Net.Api;
using System.IO;
using OldManinTheShopServer.Util;

namespace MechanicsAssistantServerTests.TestNet.TestApi
{
    [TestClass]
    public class TestUserDeleteJobs
    {
        private static HttpClient Client;
        private static MySqlDataManipulator Manipulator;
        private static QueryResponseServer Server;
        private static readonly string ConnectionString = new MySqlConnectionString("localhost", "db_test", "testUser").ConstructConnectionString("");
        private static string LoginToken;
        private static string AuthToken;
        private static readonly string SecurityQuestion = "What is your favourite colour?";
        private static readonly string Uri = "http://localhost:16384/user/job";
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
            Assert.IsTrue(Manipulator.AddJobDataToUser(Manipulator.GetUserById(1), "abc", new byte[] { 0, 2, 5 }));
        }

        [TestInitialize]
        public void FillStringConstructor()
        {
            JsonStringConstructor.SetMapping("JobId", 1);
            JsonStringConstructor.SetMapping("UserId", 1);
            JsonStringConstructor.SetMapping("AuthToken", AuthToken);
            JsonStringConstructor.SetMapping("LoginToken", LoginToken);
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
        public void TestDeleteJobIncorrectFormat()
        {
            JsonStringConstructor.RemoveMapping("LoginToken");
            string testString = JsonStringConstructor.ToString();
            StringContent content = new StringContent(testString);

            var response = Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, Uri) { Content = content }).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void TestDeleteJobJobIdOutOfRange()
        {
            JsonStringConstructor.SetMapping("JobId", 3);
            string testString = JsonStringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, Uri) { Content = content }).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void TestDeleteJobUnknownUser()
        {
            JsonStringConstructor.SetMapping("UserId", 4);
            string testString = JsonStringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, Uri) { Content = content }).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestDeleteJobInvalidLoginToken()
        {
            JsonStringConstructor.SetMapping("LoginToken", "baaaad");
            string testString = JsonStringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, Uri) { Content = content }).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestDeleteJobInvalidAuthToken()
        {
            JsonStringConstructor.SetMapping("AuthToken", "0xbaaaad");
            string testString = JsonStringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, Uri) { Content = content }).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestDeleteJobValidRequest()
        {
            string testString = JsonStringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, Uri) { Content = content }).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            var user = Manipulator.GetUserById(1);
            Assert.AreEqual(1, user.Job1Results.Length);
            Assert.AreEqual("", user.Job1Id);
        }
    }
}

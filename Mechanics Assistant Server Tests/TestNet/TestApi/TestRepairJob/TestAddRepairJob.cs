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
using MechanicsAssistantServerTests.TestNet.TestApi.TestUser;
using OldManInTheShopServer.Net.Api;
using OldManInTheShopServer.Util;
using System.IO;

namespace MechanicsAssistantServerTests.TestNet.TestApi.TestRepairJob
{
    [TestClass]
    public class TestAddRepairJob
    {
        private static HttpClient Client;
        private static MySqlDataManipulator Manipulator;
        private static QueryResponseServer Server;
        private static readonly string ConnectionString = new MySqlConnectionString("localhost", "db_test", "testUser").ConstructConnectionString("");
        private static string LoginToken;
        private static string AuthToken;
        private static readonly string SecurityQuestion = "What is your favourite colour?";
        private static readonly string Uri = "http://localhost:16384/repairjob";
        private static readonly JsonStringConstructor StringConstructor = new JsonStringConstructor();
        private static int NextId = 1;

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
            Manipulator.AddCompany("Testing Company LLC");
        }

        [TestInitialize]
        public void FillStringConstructor()
        {
            JsonStringConstructor constructor = new JsonStringConstructor();
            constructor.SetMapping("Make", "autocar");
            constructor.SetMapping("Model", "xpeditor");
            constructor.SetMapping("Complaint", "runs rough");
            constructor.SetMapping("Problem", "bad icm");
            constructor.SetMapping("Year", 1986);
            StringConstructor.SetMapping("ContainedEntry", constructor);
            StringConstructor.SetMapping("UserId", 1);
            StringConstructor.SetMapping("LoginToken", LoginToken);
            StringConstructor.SetMapping("AuthToken", AuthToken);
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
        public void TestAddRepairJobIncorrectFormat()
        {
            StringConstructor.RemoveMapping("ContainedEntry");
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void TestAddRepairJobUnknownUser()
        {
            StringConstructor.SetMapping("UserId", 3);
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestAddRepairJobInvalidLoginToken()
        {
            StringConstructor.SetMapping("LoginToken", "0");
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestAddRepairJobInvalidAuthToken()
        {
            StringConstructor.SetMapping("AuthToken", "cca");
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestAddRepairJobValidRequest()
        {
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            var entry = Manipulator.GetDataEntryById(1, NextId, false);
            Assert.AreEqual("autocar", entry.Make);
            Assert.AreEqual("xpeditor", entry.Model);
            Assert.AreEqual("runs rough", entry.Complaint);
            Assert.AreEqual("bad icm", entry.Problem);
            Assert.AreEqual(1986, entry.Year);
            NextId++;
        }

        [TestMethod]
        public void TestAddRepairJobValidRequestNoYear()
        {
            JsonStringConstructor constructor = new JsonStringConstructor();
            constructor.SetMapping("Make", "autocar");
            constructor.SetMapping("Model", "xpeditor");
            constructor.SetMapping("Complaint", "runs rough");
            constructor.SetMapping("Problem", "bad icm");
            StringConstructor.SetMapping("ContainedEntry", constructor);
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            var entry = Manipulator.GetDataEntryById(1, NextId, false);
            Assert.AreEqual("autocar", entry.Make);
            Assert.AreEqual("xpeditor", entry.Model);
            Assert.AreEqual("runs rough", entry.Complaint);
            Assert.AreEqual("bad icm", entry.Problem);
            Assert.AreEqual(-1, entry.Year);
            NextId++;
        }
    }
}

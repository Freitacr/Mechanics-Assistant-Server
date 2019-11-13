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

namespace MechanicsAssistantServerTests.TestNet.TestApi.TestUser
{
    [DataContract]
    class ExpectedLoginResponse
    {
        [DataMember(Name ="token")]
        public string Token { get; set; }
        [DataMember(Name = "userId")]
        public int Id { get; set; }
        [DataMember(Name = "accessLevel")]
        public int AccessLevel;
    }

    [TestClass]
    public class TestUserLogin
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
            StringConstructor.SetMapping("Password", "12345");
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
        public void TestLoginEmptyEmail()
        {
            StringConstructor.SetMapping("Email", "");
            string testString = StringConstructor.ToString();
            StringContent putData = new StringContent(testString);
            var response = Client.PutAsync("http://localhost:16384/user", putData);
            var actualResponse = response.Result;
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, actualResponse.StatusCode);
            Assert.AreEqual("Not all fields of the request were filled", actualResponse.Content.ReadAsStringAsync().Result);
        }

        [TestMethod]
        public void TestLoginIncorrectFormat()
        {
            StringConstructor.RemoveMapping("Password");
            string testString = StringConstructor.ToString();
            StringContent putData = new StringContent(testString);
            var response = Client.PutAsync("http://localhost:16384/user", putData);
            var actualResponse = response.Result;
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, actualResponse.StatusCode);
            Assert.AreEqual("Incorrect Format", actualResponse.ReasonPhrase);
        }

        [TestMethod]
        public void TestLoginProperFormat()
        {
            Assert.IsTrue(Manipulator.AddUser("abcd@msn", "12345", "what is your favourite colour?", "red"));

            string testString = StringConstructor.ToString();
            StringContent postData = new StringContent(testString);
            var response = Client.PutAsync("http://localhost:16384/user", postData);
            var actualResponse = response.Result;
            Assert.AreEqual(System.Net.HttpStatusCode.OK, actualResponse.StatusCode);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ExpectedLoginResponse));
            var responseContent = (ExpectedLoginResponse) serializer.ReadObject(actualResponse.Content.ReadAsStreamAsync().Result);
            Assert.AreEqual(1, responseContent.Id);
            Assert.IsTrue(UserVerificationUtil.LoginTokenValid(Manipulator.GetUserById(1), responseContent.Token));
            Assert.AreEqual(1, responseContent.AccessLevel);
        }


    }
}

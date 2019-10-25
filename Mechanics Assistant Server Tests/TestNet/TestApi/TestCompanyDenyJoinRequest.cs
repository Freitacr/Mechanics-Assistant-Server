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
using OldManInTheShopServer.Util;
using System.IO;

namespace MechanicsAssistantServerTests.TestNet.TestApi
{
    [TestClass]
    public class TestCompanyDenyJoinRequest
    {
        private static HttpClient Client;
        private static MySqlDataManipulator Manipulator;
        private static QueryResponseServer Server;
        private static readonly string ConnectionString = new MySqlConnectionString("localhost", "db_test", "testUser").ConstructConnectionString("");
        private static string LoginToken1;
        private static string LoginToken2;
        private static string LoginToken3;
        private static string LoginToken4;
        private static string LoginToken5;
        private static string AuthToken1;
        private static string AuthToken2;
        private static string AuthToken3;
        private static string AuthToken4;
        private static string AuthToken5;

        private static readonly string SecurityQuestion = "What is your favourite colour?";
        private static readonly string Uri = "http://localhost:16384/company/requests";
        private static readonly JsonStringConstructor StringConstructor = new JsonStringConstructor();



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
            Manipulator.AddUser("abcde@msn", "12345", SecurityQuestion, "red", AccessLevelMasks.AdminMask);
            Manipulator.AddUser("abcdf@msn", "12345", SecurityQuestion, "red", AccessLevelMasks.PartMask);
            Manipulator.AddUser("abcdg@msn", "12345", SecurityQuestion, "red", AccessLevelMasks.SafetyMask);
            Manipulator.AddUser("abcdh@msn", "12345", SecurityQuestion, "red", AccessLevelMasks.AdminMask | AccessLevelMasks.PartMask);
            LoginToken1 = GetLoginToken("abcd@msn", "12345");
            LoginToken2 = GetLoginToken("abcde@msn", "12345");
            LoginToken3 = GetLoginToken("abcdf@msn", "12345");
            LoginToken4 = GetLoginToken("abcdg@msn", "12345");
            LoginToken5 = GetLoginToken("abcdh@msn", "12345");

            AuthToken1 = GetAuthToken(1, LoginToken1);
            AuthToken2 = GetAuthToken(2, LoginToken2);
            AuthToken3 = GetAuthToken(3, LoginToken3);
            AuthToken4 = GetAuthToken(4, LoginToken4);
            AuthToken5 = GetAuthToken(5, LoginToken5);
            Manipulator.AddCompany("Testing Company LLC");
            Manipulator.AddDataEntry(1,
                new JobDataEntry("abc", "autocar", "xpeditor", "runs rough", "bad icm", "[]", "[]", "", 1986), true);
            Manipulator.AddJoinRequest(1, 1);
        }

        private static string GetLoginToken(string email, string password)
        {
            var content = new StringContent("{\"Email\":\"" + email + "\",\"Password\":\"" + password + "\"}");
            var response = Client.PutAsync("http://localhost:16384/user", content).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                Console.WriteLine("Test will fail due to error:" + response.Content.ReadAsStringAsync().Result);
            }
            Assert.IsTrue(response.IsSuccessStatusCode);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ExpectedLoginResponse));
            var responseContent = (ExpectedLoginResponse)serializer.ReadObject(response.Content.ReadAsStreamAsync().Result);
            return responseContent.Token;
        }

        private static string GetAuthToken(int userId, string loginToken)
        {
            var content = new StringContent("{\"UserId\":" + userId + ",\"LoginToken\":\"" + loginToken + "\",\"SecurityQuestion\":\"" + SecurityQuestion + "\",\"SecurityAnswer\":\"red\"}");
            var response = Client.PutAsync("http://localhost:16384/user/auth", content).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                Console.WriteLine("Test will fail due to error:" + response.Content.ReadAsStringAsync().Result);
            }
            Assert.IsTrue(response.IsSuccessStatusCode);
            return response.Content.ReadAsStringAsync().Result;
        }

        [TestInitialize]
        public void FillStringConstructor()
        {
            StringConstructor.SetMapping("UserId", 2);
            StringConstructor.SetMapping("LoginToken", LoginToken2);
            StringConstructor.SetMapping("AuthToken", AuthToken2);
            StringConstructor.SetMapping("RequestId", 1);
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
        public void TestDenyJoinRequestIncorrectFormat()
        {
            StringConstructor.RemoveMapping("LoginToken");
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.SendAsync(new HttpRequestMessage() { Content = content, Method = HttpMethod.Delete, RequestUri = new Uri(Uri) }).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void TestDenyJoinRequestUnknownUser()
        {
            StringConstructor.SetMapping("UserId", 7);
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.SendAsync(new HttpRequestMessage() { Content = content, Method = HttpMethod.Delete, RequestUri = new Uri(Uri) }).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestDenyJoinRequestUnauthorizedUser()
        {
            StringConstructor.SetMapping("UserId", 1);
            StringConstructor.SetMapping("LoginToken", LoginToken1);
            StringConstructor.SetMapping("AuthToken", AuthToken1);
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.SendAsync(new HttpRequestMessage() { Content = content, Method = HttpMethod.Delete, RequestUri = new Uri(Uri) }).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestDenyJoinRequestInvalidLoginToken()
        {
            StringConstructor.SetMapping("UserId", 1);
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.SendAsync(new HttpRequestMessage() { Content = content, Method = HttpMethod.Delete, RequestUri = new Uri(Uri) }).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestDenyJoinRequestInvalidAuthToken()
        {
            StringConstructor.SetMapping("AuthToken", "cca");
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.SendAsync(new HttpRequestMessage() { Content = content, Method = HttpMethod.Delete, RequestUri = new Uri(Uri) }).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestDenyJoinRequestValidRequest()
        {
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.SendAsync(new HttpRequestMessage() { Content = content, Method = HttpMethod.Delete, RequestUri = new Uri(Uri) }).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            var user = Manipulator.GetUserById(1);
            List<PreviousUserRequest> nowRequests = user.DecodeRequests();
            PreviousUserRequest partsRequest = nowRequests[0];
            Assert.AreEqual(1, partsRequest.Request.Company, "Join request company was not 1");
            Assert.AreEqual("Join", partsRequest.Request.Type);
            Assert.AreEqual("Denied", partsRequest.RequestStatus);

            List<JoinRequest> partRequests = Manipulator.GetJoinRequests(1);
            Assert.AreEqual(0, partRequests.Count);
        }
    }
}

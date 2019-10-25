﻿using System;
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
    public class TestPostToCompanyForum
    {
        private static HttpClient Client;
        private static MySqlDataManipulator Manipulator;
        private static QueryResponseServer Server;
        private static readonly string ConnectionString = new MySqlConnectionString("localhost", "db_test", "testUser").ConstructConnectionString("");
        private static string LoginToken;
        private static string AuthToken;
        private static readonly string SecurityQuestion = "What is your favourite colour?";
        private static readonly string Uri = "http://localhost:16384/company/forum";
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
            Manipulator.AddDataEntry(1,
                new JobDataEntry("abc", "autocar", "xpeditor", "runs rough", "bad icm", "[]", "[]", "", 1986), true);
        }

        [TestInitialize]
        public void FillStringConstructor()
        {
            StringConstructor.SetMapping("UserId", 1);
            StringConstructor.SetMapping("LoginToken", LoginToken);
            StringConstructor.SetMapping("AuthToken", AuthToken);
            StringConstructor.SetMapping("PostText", "Don't forget the damn wrench.");
            StringConstructor.SetMapping("CompanyId", 1);
            StringConstructor.SetMapping("JobEntryId", 1);
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
        public void TestPostForumPostRequestIncorrectFormat()
        {
            StringConstructor.RemoveMapping("PostText");
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void TestPostForumPostRequestUnknownUser()
        {
            StringConstructor.SetMapping("UserId", 7);
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestPostForumPostRequestInvalidLoginToken()
        {
            StringConstructor.SetMapping("UserId", 2);
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestPostForumPostRequestInvalidAuthToken()
        {
            StringConstructor.SetMapping("AuthToken", "cca");
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestPostForumPostRequestValidRequest()
        {
            string testString = StringConstructor.ToString();
            StringContent content = new StringContent(testString);
            var response = Client.PostAsync(Uri, content).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            List<UserToTextEntry> forumPosts = Manipulator.GetForumPosts(1, 1);
            Assert.AreEqual(1, forumPosts.Count);
            Assert.AreEqual("Don't forget the damn wrench.", forumPosts[0].Text);
            Assert.AreEqual(1, forumPosts[0].UserId);
        }
    }
}

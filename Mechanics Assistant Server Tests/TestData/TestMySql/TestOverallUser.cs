﻿using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Data.MySql;


namespace MechanicsAssistantServerTests.TestData.TestMySql
{
    [TestClass]
    public class TestOverallUser
    {
        private static MySqlConnection TestConnection;
        private static readonly string TableName = "test_user_table";

        private OverallUser Id1, Id2, Id3;

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            TestConnection = new MySqlConnection();
            TestingMySqlConnectionUtil.InitializeDatabaseSchema(
                TestConnection,
                new OverallUser().GetCreateTableString(TableName),
                TableName
            );
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            TestingMySqlConnectionUtil.DestoryDatabase();
            TestConnection.Close();
        }

        [TestInitialize]
        public void Init()
        {
            Id1 = new OverallUser()
            {
                AccessLevel = 1,
                Company = 1,
                AuthTestString = new byte[] { 1, 2, 3, 4 },
                DerivedSecurityToken = new byte[] { 5, 6, 7, 8 },
                LoginStatusTokens = "hi",
                PersonalData = new byte[] { 9, 10, 11, 12 },
                SecurityQuestion = "bye",
                Settings = "try",
                RequestHistory = new byte[] { 0 }
            };
            Id2 = new OverallUser()
            {
                AccessLevel = 1,
                Company = 1,
                AuthTestString = new byte[] { 1, 8, 3, 4 },
                DerivedSecurityToken = new byte[] { 5, 6, 7, 8 },
                LoginStatusTokens = "hi",
                PersonalData = new byte[] { 9, 10, 11, 12 },
                SecurityQuestion = "bye",
                Settings = "try",
                RequestHistory = new byte[] { 0 }
            };
            Id3 = new OverallUser()
            {
                AccessLevel = 1,
                Company = 1,
                AuthTestString = new byte[] { 1, 2, 3, 4 },
                DerivedSecurityToken = new byte[] { 5, 6, 7, 8 },
                LoginStatusTokens = "hi",
                PersonalData = new byte[] { 9, 10, 11, 12 },
                SecurityQuestion = "bye",
                Settings = "try",
                RequestHistory = new byte[] { 0 }
            };
        }

        [TestMethod]
        public void TestEquals()
        {
            Assert.AreEqual(Id1, Id3);
            Assert.AreNotEqual(Id1, Id2);
        }

        [TestMethod]
        public void TestSerialize()
        {
            Assert.AreEqual(1, OverallUser.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            Assert.AreEqual(1, OverallUser.Manipulator.RemoveDataWhere(TestConnection, TableName, "Company=" + Id1.Company));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            Assert.AreEqual(1, OverallUser.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            OverallUser toTest = OverallUser.Manipulator.RetrieveDataWhere(TestConnection, TableName, "Company=" + Id1.Company)[0];
            Assert.AreEqual(Id1, toTest);
            Assert.AreEqual(1, OverallUser.Manipulator.RemoveDataWhere(TestConnection, TableName, "Company=" + Id1.Company));
        }
    }
}

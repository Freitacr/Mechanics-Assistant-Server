﻿using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using MySql.Data.MySqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MechanicsAssistantServerTests.TestData.TestMySql
{
    [TestClass]
    public class TestPartsRequest
    {
        private static MySqlConnection TestConnection;
        private static readonly string TableName = "test_parts_request_table";

        private PartsRequest Id1, Id2, Id3;

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            TestConnection = new MySqlConnection();
            TestingMySqlConnectionUtil.InitializeDatabaseSchema(
                TestConnection,
                new PartsRequest().GetCreateTableString(TableName),
                TableName
            );
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            TestingDatabaseCreationUtils.DestoryDatabase();
            TestConnection.Close();
        }

        [TestInitialize]
        public void Init()
        {
            Id1 = new PartsRequest(1, "Whack LLC", "a, b, a");
            Id2 = new PartsRequest(1, "Mole Inc", "a, b, a");
            Id3 = new PartsRequest(1, "Whack LLC", "a, b, a");
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
            Assert.AreEqual(1, PartsRequest.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            Assert.AreEqual(1, PartsRequest.Manipulator.RemoveDataWhere(TestConnection, TableName, "ReferencedParts=\"" + Id1.ReferencedParts + "\""));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            Assert.AreEqual(1, PartsRequest.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            PartsRequest toTest = PartsRequest.Manipulator.RetrieveDataWhere(TestConnection, TableName, "ReferencedParts=\"" + Id1.ReferencedParts + "\"")[0];
            Assert.AreEqual(Id1, toTest);
            Assert.AreEqual(1, PartsRequest.Manipulator.RemoveDataWhere(TestConnection, TableName, "ReferencedParts=\"" + Id1.ReferencedParts + "\""));
        }
    }
}

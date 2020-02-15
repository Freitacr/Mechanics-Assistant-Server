using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using MySql.Data.MySqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MechanicsAssistantServerTests.TestData.TestMySql
{
    [TestClass]
    public class TestJoinRequest
    {
        private static MySqlConnection TestConnection;
        private static readonly string TableName = "test_join_request_table";

        private JoinRequest Id1, Id2, Id3;

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            TestConnection = new MySqlConnection();
            TestingMySqlConnectionUtil.InitializeDatabaseSchema(
                TestConnection,
                new JoinRequest().GetCreateTableString(TableName),
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
            Id1 = new JoinRequest(1);
            Id2 = new JoinRequest(2);
            Id3 = new JoinRequest(1);
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
            Assert.AreEqual(1, JoinRequest.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            Assert.AreEqual(1, JoinRequest.Manipulator.RemoveDataWhere(TestConnection, TableName, "UserId=" + Id1.UserId));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            Assert.AreEqual(1, JoinRequest.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            JoinRequest toTest = JoinRequest.Manipulator.RetrieveDataWhere(TestConnection, TableName, "UserId=" + Id1.UserId)[0];
            Assert.AreEqual(Id1, toTest);
            Assert.AreEqual(1, JoinRequest.Manipulator.RemoveDataWhere(TestConnection, TableName, "UserId=" + Id1.UserId));
        }
    }
}

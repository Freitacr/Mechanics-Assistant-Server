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
    public class TestRequirementAdditionRequest
    {
        private static MySqlConnection TestConnection;
        private static readonly string TableName = "test_requirement_addition_request_table";

        private RequirementAdditionRequest Id1, Id2, Id3;

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            TestConnection = new MySqlConnection();
            TestingMySqlConnectionUtil.InitializeDatabaseSchema(
                TestConnection,
                new RequirementAdditionRequest().GetCreateTableString(TableName),
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
            Id1 = new RequirementAdditionRequest(1, 2, "a");
            Id2 = new RequirementAdditionRequest(2, 2, "a");
            Id3 = new RequirementAdditionRequest(1, 2, "a");
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
            Assert.AreEqual(1, RequirementAdditionRequest.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            Assert.AreEqual(1, RequirementAdditionRequest.Manipulator.RemoveDataWhere(TestConnection, TableName, "UserId=" + Id1.UserId));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            Assert.AreEqual(1, RequirementAdditionRequest.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            RequirementAdditionRequest toTest = RequirementAdditionRequest.Manipulator.RetrieveDataWhere(TestConnection, TableName, "UserId=" + Id1.UserId)[0];
            Assert.AreEqual(Id1, toTest);
            Assert.AreEqual(1, RequirementAdditionRequest.Manipulator.RemoveDataWhere(TestConnection, TableName, "UserId=" + Id1.UserId));
        }
    }
}

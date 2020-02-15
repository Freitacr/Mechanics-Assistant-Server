using System;
using System.Collections.Generic;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using MySql.Data.MySqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MechanicsAssistantServerTests.TestData.TestMySql
{
    [TestClass]
    public class TestCompanyId
    {
        private static MySqlConnection TestConnection;
        private static readonly string TableName = "test_table";

        private CompanyId Id1, Id2, Id3;

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            
            TestConnection = new MySqlConnection();
            TestingMySqlConnectionUtil.InitializeDatabaseSchema(
                TestConnection,
                new CompanyId().GetCreateTableString(TableName),
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
            Id1 = new CompanyId("Whack LLC", .75f);
            Id2 = new CompanyId("Mole Inc", .75f);
            Id3 = new CompanyId("Whack LLC", .75f);
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
            Assert.AreEqual(1, CompanyId.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            Assert.AreEqual(1, CompanyId.Manipulator.RemoveDataWhere(TestConnection, TableName, "LegalName=\"" + Id1.LegalName + "\""));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            Assert.AreEqual(1, CompanyId.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            CompanyId toTest = CompanyId.Manipulator.RetrieveDataWhere(TestConnection, TableName, "LegalName=\"" + Id1.LegalName + "\"")[0];
            Assert.AreEqual(Id1, toTest);
            Assert.AreEqual(1, CompanyId.Manipulator.RemoveDataWhere(TestConnection, TableName, "LegalName=\"" + Id1.LegalName + "\""));
        }
    }
}

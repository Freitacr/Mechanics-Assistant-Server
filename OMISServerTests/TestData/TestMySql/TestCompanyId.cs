using System;
using System.Collections.Generic;
using MechanicsAssistantServer.Data.MySql;
using MechanicsAssistantServer.Data.MySql.TableDataTypes;
using MySql.Data.MySqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OMISServerTests.TestData.TestMySql
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
            TestConnection.ConnectionString = "SERVER=localhost;UID=testUser;";
            TestConnection.Open();
            var cmd = TestConnection.CreateCommand();
            cmd.CommandText = "create schema data_test;";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "create table " + TableName + "(id int primary key auto_increment, LegalName text);";
            cmd.ExecuteNonQuery();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            var cmd = TestConnection.CreateCommand();
            cmd.CommandText = "drop schema data_test;";
            cmd.ExecuteNonQuery();
            TestConnection.Close();
        }

        [TestInitialize]
        public void Init()
        {
            Id1 = new CompanyId("Whack LLC");
            Id2 = new CompanyId("Mole Inc");
            Id3 = new CompanyId("Whack LLC");
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
            Assert.AreEqual(1, CompanyId.READER.InsertDataInto(TestConnection, TableName, Id1));
            Assert.AreEqual(1, CompanyId.READER.RemoveDataWhere(TestConnection, TableName, "LegalName=" + Id1.LegalName));
        }
    }
}

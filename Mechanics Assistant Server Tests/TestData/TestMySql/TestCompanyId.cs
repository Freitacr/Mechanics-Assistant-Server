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
            TestConnection.ConnectionString = "SERVER=localhost;UID=testUser;PASSWORD=";
            TestConnection.Open();
            var cmd = TestConnection.CreateCommand();
            try
            {
                cmd.CommandText = "create schema data_test;";
                cmd.ExecuteNonQuery();
            } catch (MySqlException e)
            {
                if (e.Number != 1007)
                    throw e;
            }
            cmd.CommandText = "use data_test";
            cmd.ExecuteNonQuery();

            try
            {
                cmd.CommandText = "create table " + TableName + TableCreationDataDeclarationStrings.CompanyIdTable;
                cmd.ExecuteNonQuery();
            } catch (MySqlException e)
            {
                if (e.Number != 1050)
                    throw e;
                cmd.CommandText = "delete from " + TableName + ";";
                cmd.ExecuteNonQuery();
            }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            var cmd = TestConnection.CreateCommand();
            cmd.CommandText = "drop table " + TableName + ";";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "drop schema data_test;";
            cmd.ExecuteNonQuery();
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

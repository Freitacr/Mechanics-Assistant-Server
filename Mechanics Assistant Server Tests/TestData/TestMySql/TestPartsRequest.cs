using System;
using System.Collections.Generic;
using System.Text;
using MechanicsAssistantServer.Data.MySql;
using MechanicsAssistantServer.Data.MySql.TableDataTypes;
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
            TestConnection.ConnectionString = "SERVER=localhost;UID=testUser;PASSWORD=";
            TestConnection.Open();
            var cmd = TestConnection.CreateCommand();
            try
            {
                cmd.CommandText = "create schema data_test;";
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                if (e.Number != 1007)
                    throw e;
            }
            cmd.CommandText = "use data_test";
            cmd.ExecuteNonQuery();

            try
            {
                cmd.CommandText = "create table " + TableName + TableCreationDataDeclarationStrings.CompanyPartsRequest;
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
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

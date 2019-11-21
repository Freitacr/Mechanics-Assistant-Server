using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;
using MySql.Data.MySqlClient;

namespace MechanicsAssistantServerTests.TestData.TestMySql
{
    [TestClass]
    public class TestPartCatalogueEntry
    {
        private static MySqlConnection TestConnection;
        private static readonly string TableName = "test_part_catalogue_table";

        private PartCatalogueEntry Id1, Id2, Id3;

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
                cmd.CommandText = new PartCatalogueEntry().GetCreateTableString(TableName);
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
            Id1 = new PartCatalogueEntry("Autocar", "xpeditor", -1, "AX358", "Head Gasket");
            Id2 = new PartCatalogueEntry("Autocar", "xpeditor", 1998, "AX359", "Head Gasket");
            Id3 = new PartCatalogueEntry("Autocar", "xpeditor", -1, "AX358", "Head Gasket");
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
            Assert.AreEqual(1, PartCatalogueEntry.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            Assert.AreEqual(1, PartCatalogueEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "PartId=\"" + Id1.PartId + "\""));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            Assert.AreEqual(1, PartCatalogueEntry.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            PartCatalogueEntry toTest = PartCatalogueEntry.Manipulator.RetrieveDataWhere(TestConnection, TableName, "PartId=\"" + Id1.PartId + "\"")[0];
            Assert.AreEqual(Id1, toTest);
            Assert.AreEqual(1, PartCatalogueEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "PartId=\"" + Id1.PartId + "\""));
        }
    }
}

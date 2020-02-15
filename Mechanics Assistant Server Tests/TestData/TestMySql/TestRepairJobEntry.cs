using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;

namespace MechanicsAssistantServerTests.TestData.TestMySql
{
    [TestClass]
    public class TestRepairJobEntry
    {

        private static MySqlConnection TestConnection;
        private static readonly string TableName = "test_job_entry_table";

        private RepairJobEntry Id1, Id2, Id3;

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            TestConnection = new MySqlConnection();
            TestingMySqlConnectionUtil.InitializeDatabaseSchema(
                TestConnection,
                new RepairJobEntry().GetCreateTableString(TableName),
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
            Id1 = new RepairJobEntry("AX567", "autocar", "xpeditor", "Runs Rough", "bad icm", "{0, 1, 2}", "{1, 2, 3}", "Pass-Code: ICM-OVERRIDE", -1);
            Id2 = new RepairJobEntry("AX567", "autocar", "xpeditor", "Runs Rough", "cracked piston", "{0, 1, 2}", "{1, 2, 3}", "Pass-Code: ICM-OVERRIDE", -1);
            Id3 = new RepairJobEntry("AX567", "autocar", "xpeditor", "Runs Rough", "bad icm", "{0, 1, 2}", "{1, 2, 3}", "Pass-Code: ICM-OVERRIDE", -1);
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
            Assert.AreEqual(1, RepairJobEntry.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            Assert.AreEqual(1, RepairJobEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "JobId=\"" + Id1.JobId + "\""));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            Assert.AreEqual(1, RepairJobEntry.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            RepairJobEntry toTest = RepairJobEntry.Manipulator.RetrieveDataWhere(TestConnection, TableName, "JobId=\"" + Id1.JobId + "\"")[0];
            Assert.AreEqual(Id1, toTest);
            Assert.AreEqual(1, RepairJobEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "JobId=\"" + Id1.JobId + "\""));
        }

    }
}

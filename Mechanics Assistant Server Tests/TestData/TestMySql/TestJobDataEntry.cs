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
    public class TestJobDataEntry
    {

        private static MySqlConnection TestConnection;
        private static readonly string TableName = "test_job_entry_table";

        private JobDataEntry Id1, Id2, Id3;

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
                cmd.CommandText = "create table " + TableName + TableCreationDataDeclarationStrings.JobDataEntryTable;
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
            Id1 = new JobDataEntry("AX567", "autocar", "xpeditor", "Runs Rough", "bad icm", "{0, 1, 2}", "{1, 2, 3}", "Pass-Code: ICM-OVERRIDE", -1);
            Id2 = new JobDataEntry("AX567", "autocar", "xpeditor", "Runs Rough", "cracked piston", "{0, 1, 2}", "{1, 2, 3}", "Pass-Code: ICM-OVERRIDE", -1);
            Id3 = new JobDataEntry("AX567", "autocar", "xpeditor", "Runs Rough", "bad icm", "{0, 1, 2}", "{1, 2, 3}", "Pass-Code: ICM-OVERRIDE", -1);
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
            Assert.AreEqual(1, JobDataEntry.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            Assert.AreEqual(1, JobDataEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "JobId=\"" + Id1.JobId + "\""));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            Assert.AreEqual(1, JobDataEntry.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            JobDataEntry toTest = JobDataEntry.Manipulator.RetrieveDataWhere(TestConnection, TableName, "JobId=\"" + Id1.JobId + "\"")[0];
            Assert.AreEqual(Id1, toTest);
            Assert.AreEqual(1, JobDataEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "JobId=\"" + Id1.JobId + "\""));
        }

    }
}

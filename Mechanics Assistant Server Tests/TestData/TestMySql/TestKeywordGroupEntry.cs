using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MechanicsAssistantServer.Data.MySql.TableDataTypes;
using MySql.Data.MySqlClient;

namespace MechanicsAssistantServerTests.TestData.TestMySql
{
    [TestClass]
    public class TestKeywordGroupEntry
    {
        private static MySqlConnection TestConnection;
        private static readonly string TableName = "test_keyword_group_table";

        private KeywordGroupEntry Id1, Id2, Id3;

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
                cmd.CommandText = "create table " + TableName + "(id int primary key auto_increment, GroupDefinition text);";
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
            Id1 = new KeywordGroupEntry("Whack LLC");
            Id2 = new KeywordGroupEntry("Mole Inc");
            Id3 = new KeywordGroupEntry("Whack LLC");
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
            Assert.AreEqual(1, KeywordGroupEntry.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            Assert.AreEqual(1, KeywordGroupEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "GroupDefinition=\"" + Id1.GroupDefinition + "\""));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            Assert.AreEqual(1, KeywordGroupEntry.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            KeywordGroupEntry toTest = KeywordGroupEntry.Manipulator.RetrieveDataWhere(TestConnection, TableName, "GroupDefinition=\"" + Id1.GroupDefinition + "\"")[0];
            Assert.AreEqual(Id1, toTest);
            Assert.AreEqual(1, KeywordGroupEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "GroupDefinition=\"" + Id1.GroupDefinition + "\""));
        }
    }
}

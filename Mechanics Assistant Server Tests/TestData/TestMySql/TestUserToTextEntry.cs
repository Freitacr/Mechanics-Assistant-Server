using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using MechanicsAssistantServer.Data.MySql.TableDataTypes;
using MechanicsAssistantServer.Data.MySql;

namespace MechanicsAssistantServerTests.TestData.TestMySql
{
    [TestClass]
    public class TestUserToTextEntry
    {
        private static MySqlConnection TestConnection;
        private static readonly string TableName = "test_user_to_text_table";

        private UserToTextEntry Id1, Id2, Id3;

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
                cmd.CommandText = "create table " + TableName + TableCreationDataDeclarationStrings.UserEmailTable;
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
            Id1 = new UserToTextEntry(1, "a@b.com");
            Id2 = new UserToTextEntry(2, "a@c.com");
            Id3 = new UserToTextEntry(1, "a@b.com");
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
            Assert.AreEqual(1, UserToTextEntry.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            Assert.AreEqual(1, UserToTextEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "MappedText=\"" + Id1.Text + "\""));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            Assert.AreEqual(1, UserToTextEntry.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            UserToTextEntry toTest = UserToTextEntry.Manipulator.RetrieveDataWhere(TestConnection, TableName, "MappedText=\"" + Id1.Text + "\"")[0];
            Assert.AreEqual(Id1, toTest);
            Assert.AreEqual(1, UserToTextEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "MappedText=\"" + Id1.Text + "\""));
        }
    }
}

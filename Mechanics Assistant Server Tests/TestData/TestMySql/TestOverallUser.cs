using System;
using System.Collections.Generic;
using System.Text;
using MechanicsAssistantServer.Data.MySql.TableDataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;


namespace MechanicsAssistantServerTests.TestData.TestMySql
{
    [TestClass]
    public class TestOverallUser
    {
        private static MySqlConnection TestConnection;
        private static readonly string TableName = "test_user_table";

        private OverallUser Id1, Id2, Id3;

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
                cmd.CommandText = "create table " + TableName + "(id int primary key auto_increment, AccessLevel int," +
                    " DerivedSecurityToken varbinary(64), SecurityQuestion text, PersonalData varbinary(1024), Settings text," +
                    " Company int, AuthToken varbinary(64), LoggedToken text);";
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
            Id1 = new OverallUser()
            {
                AccessLevel = 1,
                Company = 1,
                AuthToken = new byte[] { 1, 2, 3, 4 },
                DerivedSecurityToken = new byte[] { 5, 6, 7, 8 },
                LoggedToken = "hi",
                PersonalData = new byte[] { 9, 10, 11, 12 },
                SecurityQuestion = "bye",
                Settings = "try"
            };
            Id2 = new OverallUser()
            {
                AccessLevel = 1,
                Company = 1,
                AuthToken = new byte[] { 1, 8, 3, 4 },
                DerivedSecurityToken = new byte[] { 5, 6, 7, 8 },
                LoggedToken = "hi",
                PersonalData = new byte[] { 9, 10, 11, 12 },
                SecurityQuestion = "bye",
                Settings = "try"
            };
            Id3 = new OverallUser()
            {
                AccessLevel = 1,
                Company = 1,
                AuthToken = new byte[] { 1, 2, 3, 4 },
                DerivedSecurityToken = new byte[] { 5, 6, 7, 8 },
                LoggedToken = "hi",
                PersonalData = new byte[] { 9, 10, 11, 12 },
                SecurityQuestion = "bye",
                Settings = "try"
            };
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
            Assert.AreEqual(1, OverallUser.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            Assert.AreEqual(1, OverallUser.Manipulator.RemoveDataWhere(TestConnection, TableName, "Company=" + Id1.Company));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            Assert.AreEqual(1, OverallUser.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            OverallUser toTest = OverallUser.Manipulator.RetrieveDataWhere(TestConnection, TableName, "Company=" + Id1.Company)[0];
            Assert.AreEqual(Id1, toTest);
            Assert.AreEqual(1, OverallUser.Manipulator.RemoveDataWhere(TestConnection, TableName, "Company=" + Id1.Company));
        }
    }
}

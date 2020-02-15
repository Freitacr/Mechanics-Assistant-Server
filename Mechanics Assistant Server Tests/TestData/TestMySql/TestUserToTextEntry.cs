using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;

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
            TestingMySqlConnectionUtil.InitializeDatabaseSchema(TestConnection, 
                new UserToTextEntry().GetCreateTableString(TableName), TableName);
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
            Assert.AreEqual(1, UserToTextEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "Text=\"" + Id1.Text + "\""));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            Assert.AreEqual(1, UserToTextEntry.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            UserToTextEntry toTest = UserToTextEntry.Manipulator.RetrieveDataWhere(TestConnection, TableName, "Text=\"" + Id1.Text + "\"")[0];
            Assert.AreEqual(Id1, toTest);
            Assert.AreEqual(1, UserToTextEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "Text=\"" + Id1.Text + "\""));
        }
    }
}

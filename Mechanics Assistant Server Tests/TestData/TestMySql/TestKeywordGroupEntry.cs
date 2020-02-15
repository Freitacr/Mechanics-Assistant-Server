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
    public class TestKeywordGroupEntry
    {
        private static MySqlConnection TestConnection;
        private static readonly string TableName = "test_keyword_group_table";

        private KeywordGroupEntry Id1, Id2, Id3;

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            TestConnection = new MySqlConnection();
            TestingMySqlConnectionUtil.InitializeDatabaseSchema(
                TestConnection,
                new KeywordGroupEntry().GetCreateTableString(TableName),
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

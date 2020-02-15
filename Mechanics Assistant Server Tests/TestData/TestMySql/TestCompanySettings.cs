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
    public class TestCompanySettings
    {
        private static MySqlConnection TestConnection;
        private static readonly string TableName = "test_company_setting_table";

        private CompanySettingsEntry Id1, Id2, Id3;

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            TestConnection = new MySqlConnection();
            TestingMySqlConnectionUtil.InitializeDatabaseSchema(
                TestConnection,
                new CompanySettingsEntry().GetCreateTableString(TableName),
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
            Id1 = new CompanySettingsEntry("a@b.com", "1");
            Id2 = new CompanySettingsEntry("a@c.com", "1");
            Id3 = new CompanySettingsEntry("a@b.com", "1");
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
            Assert.AreEqual(1, CompanySettingsEntry.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            Assert.AreEqual(1, CompanySettingsEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "SettingKey=\"" + Id1.SettingKey + "\""));
        }

        [TestMethod]
        public void TestDeserialize()
        {
            Assert.AreEqual(1, CompanySettingsEntry.Manipulator.InsertDataInto(TestConnection, TableName, Id1));
            CompanySettingsEntry toTest = CompanySettingsEntry.Manipulator.RetrieveDataWhere(TestConnection, TableName, "SettingKey=\"" + Id1.SettingKey + "\"")[0];
            Assert.AreEqual(Id1, toTest);
            Assert.AreEqual(1, CompanySettingsEntry.Manipulator.RemoveDataWhere(TestConnection, TableName, "SettingKey=\"" + Id1.SettingKey + "\""));
        }
    }
}

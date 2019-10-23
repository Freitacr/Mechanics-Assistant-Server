using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using OldManinTheShopServer.Data.MySql.TableDataTypes;
using OldManinTheShopServer.Data.MySql;

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
                cmd.CommandText = "create table " + TableName + TableCreationDataDeclarationStrings.CompanySettings;
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

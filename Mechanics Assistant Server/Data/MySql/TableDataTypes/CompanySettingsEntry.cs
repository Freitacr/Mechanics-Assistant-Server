using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace MechanicsAssistantServer.Data.MySql.TableDataTypes
{
    class CompanySettingsEntry : ISqlSerializable
    {
        public static readonly TableDataManipulator<CompanySettingsEntry> Manipulator = new TableDataManipulator<CompanySettingsEntry>();

        public string SettingKey { get; set; }
        public string SettingValue { get; set; }

        public int Id { get; set; }

        public CompanySettingsEntry()
        {

        }

        public CompanySettingsEntry(string key, string value)
        {
            SettingKey = key;
            SettingValue = value;
        }

        public ISqlSerializable Copy()
        {
            return new CompanySettingsEntry(SettingKey, SettingValue);
        }

        public void Deserialize(MySqlDataReader reader)
        {
            SettingKey = (string)reader["SettingKey"];
            SettingValue = (string)reader["SettingValue"];
            Id = (int)reader["id"];
        }

        public string Serialize(string tableName)
        {
            return "insert into " + tableName + "(SettingKey, SettingValue) values(" +
                "\"" + SettingKey + "\",\"" + SettingValue + "\");";
        }

        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return (obj as CompanySettingsEntry).GetHashCode() == GetHashCode();
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return SettingKey.GetHashCode() + SettingValue.GetHashCode();
        }
    }
}

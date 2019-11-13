using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    public static class CompanySettingsKey
    {
        public static string RetrainInterval { get; } = "Retrain Interval";
        public static string Public { get; } = "Is Public";
        public static string KeywordPredictor { get; } = "Keyword Prediction Model";
        public static string KeywordClusterer { get; } = "Keyword Clusterer Model";
        public static string ProblemPredictor { get; } = "Problem Prediction Model";
        public static string Downvotes { get; } = "Downvotes before Requirement Deletion";
    }

    public static class CompanySettingsOptions
    {
        public static JsonListStringConstructor RetrainInterval { get; } = ConstructOptionsString(new[] { "5", "6", "7", "8", "9" });
        public static JsonListStringConstructor Downvotes { get; } = ConstructOptionsString(new[] { "5", "4", "3", "2", "1" });
        public static JsonListStringConstructor Public { get; } = ConstructOptionsString(new[] { true.ToString(), false.ToString() });
        public static JsonListStringConstructor KeywordPredictor { get; } = ConstructOptionsString(new[] { "Bayesian" });
        public static JsonListStringConstructor ProblemPredictor { get; } = ConstructOptionsString(new[] { "Database" });
        public static JsonListStringConstructor KeywordClusterer { get; } = ConstructOptionsString(new[] { "Similiarity" });

        private static JsonListStringConstructor ConstructOptionsString(IEnumerable<string> options)
        {
            JsonListStringConstructor constructor = new JsonListStringConstructor();
            foreach (string s in options)
                constructor.AddElement(s);
            return constructor;
        }

    }

    public class CompanySettingsEntry : ISqlSerializable
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

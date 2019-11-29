using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Util;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    /// <summary>
    /// Static class containing the keys for any valid Company Setting
    /// </summary>
    public static class CompanySettingsKey
    {
        public static string RetrainInterval { get; } = "Retrain Interval";
        public static string Public { get; } = "Is Data Public";
        public static string KeywordPredictor { get; } = "Keyword Prediction Model";
        public static string KeywordClusterer { get; } = "Keyword Clusterer Model";
        public static string ProblemPredictor { get; } = "Problem Prediction Model";
        public static string Downvotes { get; } = "Downvotes before Requirement Deletion";
        public static string DataUploadable { get; } = "Can Mechanics Upload Data";
    }

    /// <summary>
    /// Static class containing the valid options a Company Setting may be. Based on the same naming as CompanySettingsKey
    /// </summary>
    public static class CompanySettingsOptions
    {
        public static JsonListStringConstructor RetrainInterval { get; } = ConstructOptionsString(new[] { "5", "6", "7", "8", "9" });
        public static JsonListStringConstructor Downvotes { get; } = ConstructOptionsString(new[] { "5", "4", "3", "2", "1" });
        public static JsonListStringConstructor Public { get; } = ConstructOptionsString(new[] { true.ToString(), false.ToString() });
        public static JsonListStringConstructor KeywordPredictor { get; } = ConstructOptionsString(new[] { "Bayesian" });
        public static JsonListStringConstructor ProblemPredictor { get; } = ConstructOptionsString(new[] { "Database" });
        public static JsonListStringConstructor KeywordClusterer { get; } = ConstructOptionsString(new[] { "Similiarity" });
        public static JsonListStringConstructor DataUploadable { get; } = ConstructOptionsString(new[] { true.ToString(), false.ToString() });

        private static JsonListStringConstructor ConstructOptionsString(IEnumerable<string> options)
        {
            JsonListStringConstructor constructor = new JsonListStringConstructor();
            foreach (string s in options)
                constructor.AddElement(s);
            return constructor;
        }

    }

    /// <summary>
    /// Class that represents a setting that applies on the company-wide scope, and is stored in the company's settings table
    /// </summary>
    public class CompanySettingsEntry : MySqlTableDataMember<CompanySettingsEntry>
    {
        public static readonly TableDataManipulator<CompanySettingsEntry> Manipulator = new TableDataManipulator<CompanySettingsEntry>();

        /// <summary>
        /// The unique key used to identify this setting
        /// </summary>
        [SqlTableMember("varchar(64)", MySqlDataFormatString = "\"{0}\"")]
        public string SettingKey;

        /// <summary>
        /// The value of this setting
        /// </summary>
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string SettingValue;

        /// <summary>
        /// Default constructor, required by <see cref="TableDataManipulator{T}"/>
        /// </summary>
        public CompanySettingsEntry()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">Unique key used to identify this setting</param>
        /// <param name="value">The value of this setting</param>
        public CompanySettingsEntry(string key, string value)
        {
            SettingKey = key;
            SettingValue = value;
        }

        public override ISqlSerializable Copy()
        {
            return new CompanySettingsEntry(SettingKey, SettingValue);
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

        protected override void ApplyDefaults()
        {
            SettingKey = null;
            SettingValue = null;
        }

        public override string ToString()
        {
            return SettingKey + ": " + SettingValue;
        }
    }
}

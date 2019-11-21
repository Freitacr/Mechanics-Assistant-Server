using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Util;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Security.Cryptography;
using ANSEncodingLib;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    [DataContract]
    public class LoggedTokens
    {
        [DataMember]
        public string BaseLoggedInToken { get; set; } = "";

        [DataMember]
        public string BaseLoggedInTokenExpiration { get; set; } = "";

        [DataMember]
        public string AuthLoggedInToken { get; set; } = "";

        [DataMember]
        public string AuthLoggedInTokenExpiration { get; set; } = "";
    }

    [DataContract]
    class UserSettingsEntry
    {
        [DataMember]
        public string Key { get; set; } = "";

        [DataMember]
        public string Value { get; set; } = "";
    }

    public static class UserSettingsEntryKeys
    {
        public static readonly string DisplayName = "Display Name";
        public static readonly string ComplaintGroupResults = "Number Complaint Group Results";
        public static readonly string ProblemGroupResults = "Number Problem Group Results";
        public static readonly string PredictionQueryResults = "Number Prediction Filtered Results";
        public static readonly string ArchiveQueryResults = "Number Archive Filtered Results";
    }

    [DataContract]
    public class RequestString
    {
        [DataMember]
        public int Company { get; set; }
        
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string MD5 { get; set; }

        public void CalculateMD5(string extraData = "")
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            using (md5)
            {
                byte[] data = Encoding.UTF8.GetBytes(Type + Company + extraData);
                MD5 = MysqlDataConvertingUtil.ConvertToHexString(md5.ComputeHash(data));
            }
        }
    }

    [DataContract]
    public class PreviousUserRequest
    {
        [DataMember]
        public RequestString Request { get; set; }

        [DataMember]
        public string RequestStatus { get; set; } = "";
    }

    public class OverallUser : MySqlTableDataMember<OverallUser>
    {
        public static string GenerateDefaultSettings()
        {
            List<UserSettingsEntry> entries = new List<UserSettingsEntry>();
            entries.Add(new UserSettingsEntry() { Key = UserSettingsEntryKeys.DisplayName, Value = "defaultUser" });
            entries.Add(new UserSettingsEntry() { Key = UserSettingsEntryKeys.ProblemGroupResults, Value = "4" });
            entries.Add(new UserSettingsEntry() { Key = UserSettingsEntryKeys.ArchiveQueryResults, Value = "20" });
            entries.Add(new UserSettingsEntry() { Key = UserSettingsEntryKeys.ComplaintGroupResults, Value = "4" });
            entries.Add(new UserSettingsEntry() { Key = UserSettingsEntryKeys.PredictionQueryResults, Value = "20" });
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<UserSettingsEntry>));
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, entries);
            byte[] outBytes = stream.ToArray();
            return Encoding.UTF8.GetString(outBytes);
        }

        
        public static readonly TableDataManipulator<OverallUser> Manipulator = new TableDataManipulator<OverallUser>();

        [SqlTableMember("int")]
        public int AccessLevel;

        [SqlTableMember("varbinary(64)")]
        public byte[] DerivedSecurityToken;

        [SqlTableMember("varchar(256)", MySqlDataFormatString = "\"{0}\"")]
        public string SecurityQuestion;

        [SqlTableMember("varbinary(1024)")]
        public byte[] PersonalData;

        [SqlTableMember("varchar(512)", MySqlDataFormatString = "\"{0}\"")]
        public string Settings;

        [SqlTableMember("int")]
        public int Company;

        [SqlTableMember("varbinary(64)")]
        public byte[] AuthToken;

        [SqlTableMember("varchar(512)", MySqlDataFormatString = "\"{0}\"")]
        public string LoggedTokens;

        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string Email;

        public static int RequestHistoryBytesSize = 2048;

        [SqlTableMember("varbinary(2048)")]
        public byte[] RequestHistory;

        [SqlTableMember("int")]
        public int UserId;

        public OverallUser()
        {
        }

        public override ISqlSerializable Copy()
        {
            OverallUser ret = new OverallUser
            {
                AccessLevel = AccessLevel,
                DerivedSecurityToken = (byte[])DerivedSecurityToken.Clone(),
                SecurityQuestion = SecurityQuestion,
                PersonalData = (byte[])PersonalData.Clone(),
                Settings = Settings,
                Company = Company,
                AuthToken = (byte[])AuthToken.Clone(),
                LoggedTokens = LoggedTokens,
                Email = Email,
                RequestHistory = RequestHistory
            };
            return ret;
        }
        public bool UpdateSettings(string key, string value)
        {
            byte[] settings = Encoding.UTF8.GetBytes(Settings);
            MemoryStream stream = new MemoryStream(settings);
            var serializer = new DataContractJsonSerializer(typeof(List<UserSettingsEntry>));
            List<UserSettingsEntry> settingsEntries = (List<UserSettingsEntry>)serializer.ReadObject(stream);
            foreach(UserSettingsEntry entry in settingsEntries) {
                if(entry.Key.Equals(key))
                {
                    entry.Value = value;
                    stream = new MemoryStream();
                    serializer.WriteObject(stream, settingsEntries);
                    settings = stream.ToArray();
                    Settings = Encoding.UTF8.GetString(settings);
                    return true;
                }
            }
            return false;
        }

        public List<PreviousUserRequest> DecodeRequests()
        {
            MemoryStream streamOut = new MemoryStream();
            AnsBlockDecoder decoder = new AnsBlockDecoder(streamOut);
            MemoryStream streamIn = new MemoryStream(RequestHistory);
            decoder.DecodeStream(streamIn);
            streamIn = new MemoryStream(streamOut.ToArray());
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<PreviousUserRequest>));
            return serializer.ReadObject(streamIn) as List<PreviousUserRequest>;
        }

        public void EncodeRequests(List<PreviousUserRequest> requestsIn)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<PreviousUserRequest>));
            MemoryStream streamOut = new MemoryStream();
            serializer.WriteObject(streamOut, requestsIn);
            MemoryStream streamIn = new MemoryStream(streamOut.ToArray());
            streamOut = new MemoryStream();
            EncodingUtilities.BitWriter writer = new EncodingUtilities.BitWriter(streamOut);
            AnsBlockEncoder encoder = new AnsBlockEncoder(1024, writer);
            encoder.EncodeStream(streamIn, 8);
            writer.Flush();
            RequestHistory = streamOut.ToArray();
        }

        public override void Deserialize(MySqlDataReader reader)
        {
            AccessLevel = (int)reader["AccessLevel"];
            DerivedSecurityToken = (byte[])reader["DerivedSecurityToken"];
            SecurityQuestion = (string)reader["SecurityQuestion"];
            PersonalData = (byte[])reader["PersonalData"];
            Settings = (string)reader["Settings"];
            Company = (int)reader["Company"];
            AuthToken = (byte[])reader["AuthToken"];
            LoggedTokens = (string)reader["LoggedTokens"];
            Email = (string)reader["Email"];
            RequestHistory = (byte[])reader["RequestHistory"];
            UserId = (int)reader["id"];
        }

        public override string Serialize(string tableName)
        {
            StringBuilder retBuilder = new StringBuilder();
            retBuilder.Append("insert into ");
            retBuilder.Append(tableName);
            retBuilder.Append("(AccessLevel, DerivedSecurityToken, SecurityQuestion, PersonalData, Settings, Company, AuthToken, LoggedTokens, Email, RequestHistory) Values (");
            retBuilder.Append(AccessLevel);
            retBuilder.Append(",");
            retBuilder.Append(MysqlDataConvertingUtil.ConvertToHexString(DerivedSecurityToken));
            retBuilder.Append(",");
            retBuilder.Append("\"" + SecurityQuestion + "\"");
            retBuilder.Append(",");
            retBuilder.Append(MysqlDataConvertingUtil.ConvertToHexString(PersonalData));
            retBuilder.Append(",");
            retBuilder.Append("\"" + Settings + "\"");
            retBuilder.Append(",");
            retBuilder.Append(Company);
            retBuilder.Append(",");
            retBuilder.Append(MysqlDataConvertingUtil.ConvertToHexString(AuthToken));
            retBuilder.Append(",");
            retBuilder.Append("\"" + LoggedTokens + "\"");
            retBuilder.Append(",");
            retBuilder.Append("\"" + Email + "\"");
            retBuilder.Append(",");
            retBuilder.Append(MysqlDataConvertingUtil.ConvertToHexString(RequestHistory));
            retBuilder.Append(");");
            return retBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var other = obj as OverallUser;
            if (other.AuthToken.Length != AuthToken.Length)
                return false;
            if (other.DerivedSecurityToken.Length != DerivedSecurityToken.Length)
                return false;

            for(int i = 0; i < other.AuthToken.Length; i++)
            {
                if (other.AuthToken[i] != AuthToken[i])
                    return false;
            }

            for (int i = 0; i < other.DerivedSecurityToken.Length; i++)
            {
                if (other.DerivedSecurityToken[i] != DerivedSecurityToken[i])
                    return false;
            }
            //Testing the two security tokens should be more than sufficient. It may produce a
            //RARE false positive, but that is so far from likely.
            return true;
        }

        public override int GetHashCode()
        {
            return Company + AccessLevel + SecurityQuestion.GetHashCode() + LoggedTokens.GetHashCode();
        }

        protected override void ApplyDefaults()
        {
        }

        public override string ToString()
        {
            return Email ?? "";
        }
    }
}

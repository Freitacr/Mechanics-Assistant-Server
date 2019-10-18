using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using MechanicsAssistantServer.Util;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using ANSEncodingLib;

namespace MechanicsAssistantServer.Data.MySql.TableDataTypes
{
    [DataContract]
    class LoggedTokens
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
    class SettingsEntry
    {
        [DataMember]
        public string Key { get; set; } = "";

        [DataMember]
        public string Value { get; set; } = "";
    }

    [DataContract]
    class PreviousUserRequest
    {
        [DataMember]
        public string Request { get; set; } = "";

        [DataMember]
        public string RequestStatus { get; set; } = "";
    }

    class OverallUser : ISqlSerializable
    {
        public static string GenerateDefaultSettings()
        {
            List<SettingsEntry> entries = new List<SettingsEntry>();
            entries.Add(new SettingsEntry() { Key = "displayName", Value = "defaultUser" });
            entries.Add(new SettingsEntry() { Key = "numPredictResultGroups", Value = "4" });
            entries.Add(new SettingsEntry() { Key = "numPredictResultEntries", Value = "20" });
            entries.Add(new SettingsEntry() { Key = "numArchiveResultGroups", Value = "4" });
            entries.Add(new SettingsEntry() { Key = "numArchiveResultEntries", Value = "20" });
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<SettingsEntry>));
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, entries);
            byte[] outBytes = stream.ToArray();
            return Encoding.UTF8.GetString(outBytes);
        }

        
        public static readonly TableDataManipulator<OverallUser> Manipulator = new TableDataManipulator<OverallUser>();

        public int AccessLevel { get; set; }
        public byte[] DerivedSecurityToken { get; set; }
        public string SecurityQuestion { get; set; }
        public byte[] PersonalData { get; set; }
        public string Settings { get; set; }
        public int Company { get; set; }
        public byte[] AuthToken { get; set; }
        public string LoggedTokens { get; set; }
        public string Job1Id { get; set; }
        public string Job2Id { get; set; }
        public byte[] Job1Results { get; set; }
        public byte[] Job2Results { get; set; }
        public string Email { get; set; }
        public byte[] RequestHistory { get; set; }
        public int UserId { get; set; }

        public OverallUser()
        {
            Job1Results = new byte[] { 0 };
            Job2Results = new byte[] { 0 };
        }

        public ISqlSerializable Copy()
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
                Job1Id = Job1Id,
                Job2Id = Job2Id,
                Job1Results = Job1Results,
                Job2Results = Job2Results,
                Email = Email,
                RequestHistory = RequestHistory
            };
            return ret;
        }
        public bool UpdateSettings(string key, string value)
        {
            byte[] settings = Encoding.UTF8.GetBytes(Settings);
            MemoryStream stream = new MemoryStream(settings);
            var serializer = new DataContractJsonSerializer(typeof(List<SettingsEntry>));
            List<SettingsEntry> settingsEntries = (List<SettingsEntry>)serializer.ReadObject(stream);
            foreach(SettingsEntry entry in settingsEntries) {
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

        public void Deserialize(MySqlDataReader reader)
        {
            AccessLevel = (int)reader["AccessLevel"];
            DerivedSecurityToken = (byte[])reader["DerivedSecurityToken"];
            SecurityQuestion = (string)reader["SecurityQuestion"];
            PersonalData = (byte[])reader["PersonalData"];
            Settings = (string)reader["Settings"];
            Company = (int)reader["Company"];
            AuthToken = (byte[])reader["AuthToken"];
            LoggedTokens = (string)reader["LoggedToken"];
            Email = (string)reader["Email"];
            RequestHistory = (byte[])reader["RequestHistory"];
            Job1Id = (string)reader["Job1Id"];
            Job2Id = (string)reader["Job2Id"];
            Job1Results = (byte[])reader["Job1Results"];
            Job2Results = (byte[])reader["Job2Results"];
            UserId = (int)reader["id"];
        }

        public string Serialize(string tableName)
        {
            StringBuilder retBuilder = new StringBuilder();
            retBuilder.Append("insert into ");
            retBuilder.Append(tableName);
            retBuilder.Append("(AccessLevel, DerivedSecurityToken, SecurityQuestion, PersonalData, Settings, Company, AuthToken, LoggedToken, Job1Id, Job2Id, Job1Results, Job2Results, Email, RequestHistory) Values (");
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
            retBuilder.Append("\"" + Job1Id + "\"");
            retBuilder.Append(",");
            retBuilder.Append("\"" + Job2Id + "\"");
            retBuilder.Append(",");
            retBuilder.Append(MysqlDataConvertingUtil.ConvertToHexString(Job1Results));
            retBuilder.Append(",");
            retBuilder.Append(MysqlDataConvertingUtil.ConvertToHexString(Job2Results));
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
    }
}

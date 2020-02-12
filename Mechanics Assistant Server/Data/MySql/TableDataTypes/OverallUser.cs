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
    /// <summary>
    /// Class that represents the JSON format of the LoginStatusTokens a user has in the database
    /// </summary>
    [DataContract]
    public class LoginStatusTokens
    {
        /// <summary>
        /// MySql Binary Literal representing the random 64 byte token a user receives when they log in successfully
        /// </summary>
        [DataMember]
        public string LoginToken { get; set; } = "";

        /// <summary>
        /// UTC string representing when the user's LoginToken expires
        /// </summary>
        [DataMember]
        public string LoginTokenExpiration { get; set; } = "";

        /// <summary>
        /// MySql Binary Literal representing the random 64 byte token a user receives when they successfully authenticate themselves
        /// </summary>
        [DataMember]
        public string AuthToken { get; set; } = "";

        /// <summary>
        /// UTC string representing when the user's AuthToken expires
        /// </summary>
        [DataMember]
        public string AuthTokenExpiration { get; set; } = "";
    }

    /// <summary>
    /// Class representing the JSON format a user's setting should take
    /// </summary>
    [DataContract]
    class UserSettingsEntry
    {
        /// <summary>
        /// The unique key used to identify the setting
        /// </summary>
        [DataMember]
        public string Key { get; set; } = "";

        /// <summary>
        /// The value of the setting
        /// </summary>
        [DataMember]
        public string Value { get; set; } = "";
    }

    /// <summary>
    /// A static class used to store the valid Keys for a User Setting
    /// </summary>
    public static class UserSettingsEntryKeys
    {
        public static readonly string DisplayName = "Display Name";
        public static readonly string ComplaintGroupResults = "Number Complaint Group Results";
        public static readonly string ProblemGroupResults = "Number Problem Group Results";
        public static readonly string PredictionQueryResults = "Number Prediction Filtered Results";
        public static readonly string ArchiveQueryResults = "Number Archive Filtered Results";
    }

    /// <summary>
    /// <para>Class used to represent the JSON format of a request in the user's request history</para>
    /// This class does not contain the status of the request, only the information about the request
    /// </summary>
    [DataContract]
    public class RequestString
    {
        /// <summary>
        /// The company the request was made to
        /// </summary>
        [DataMember]
        public int Company { get; set; }

        /// <summary>
        /// <para>The type of request made</para>
        /// Valid values are: "join", "parts", "partslist", and "safety"
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Message digest of the request, used to identify the request quickly
        /// </summary>
        [DataMember]
        public string MD5 { get; set; }

        /// <summary>
        /// Calculates the message digest of the request along with the extra data passe din
        /// </summary>
        /// <param name="extraData">Extra data to store with the request (such as the list of parts requested for a parts request)</param>
        public void CalculateMD5(string extraData = "")
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            using (md5)
            {
                byte[] data = Encoding.UTF8.GetBytes(Type + Company + extraData);
                MD5 = MysqlDataConvertingUtil.ConvertToHexString(md5.ComputeHash(data));
            }
        }

        // override object.Equals
        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = obj as RequestString;

            return other.Company == Company && other.Type == Type;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Company + Type.GetHashCode();
        }
    }

    /// <summary>
    /// Class representing the JSON format of a request that the user has made stored in their history.
    /// Includes the status of the request as well.
    /// </summary>
    [DataContract]
    public class PreviousUserRequest
    {
        /// <summary>
        /// RequestString object representing the request made
        /// </summary>
        [DataMember]
        public RequestString Request { get; set; }

        /// <summary>
        /// The current status of the request
        /// </summary>
        [DataMember]
        public string RequestStatus { get; set; } = "";

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var other = obj as PreviousUserRequest;
            return other.Request.Equals(Request) && RequestStatus == other.RequestStatus;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Request.GetHashCode() + RequestStatus.GetHashCode();
        }
    }

    /// <summary>
    /// Class representing a user and how they are stored in the database
    /// </summary>
    public class OverallUser : MySqlTableDataMember<OverallUser>
    {
        /// <summary>
        /// Generates the default settings for a user
        /// </summary>
        /// <returns>JSON formatted string representing the user's settings</returns>
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

        /// <summary>
        /// <param>Int representing the access level of the user</param>
        /// For more information on access levels see <see cref="AccessLevelMasks"/>
        /// </summary>
        [SqlTableMember("int")]
        public int AccessLevel;

        /// <summary>
        /// <para>64 byte array containing the security token used for authenticating a user</para>
        /// For more information on the creation of the DST see <see cref="OMISSecLib.SecuritySchemaLib.ConstructDerivedSecurityToken"/>
        /// </summary>
        [SqlTableMember("varbinary(64)")]
        public byte[] DerivedSecurityToken;

        /// <summary>
        /// <para>Security question that a user must answer in order to successfully authenticate themselves</para>
        /// For more information on the authentication process see <see cref="UserVerificationUtil.VerifyAuthentication"/>
        /// </summary>
        [SqlTableMember("varchar(256)", MySqlDataFormatString = "\"{0}\"")]
        public string SecurityQuestion;

        /// <summary>
        /// A 1024 long byte array containing the user's personal data. This is currently unused, and is likely to be removed in the future
        /// </summary>
        [SqlTableMember("varbinary(1024)")]
        public byte[] PersonalData;

        /// <summary>
        /// A string of maximum 512 characters representing the user's settings.
        /// </summary>
        [SqlTableMember("varchar(512)", MySqlDataFormatString = "\"{0}\"")]
        public string Settings;

        /// <summary>
        /// Database id of the company the user is a part of
        /// </summary>
        [SqlTableMember("int")]
        public int Company;

        /// <summary>
        /// <para>A byte array of maximum 64 bytes representing the string a user must construct in order to pass authentication</para>
        /// <para>For more information regarding creation of this string see <see cref="MySqlDataManipulator.AddUser"/></para>
        /// <para>For more information on how a user passes authentication, see <see cref="UserVerificationUtil.VerifyAuthentication"/></para>
        /// </summary>
        [SqlTableMember("varbinary(64)")]
        public byte[] AuthTestString;

        /// <summary>
        /// <para>JSON formatted string representing the LoginStatusTokens associated with this user</para>
        /// <para>For the format of this JSON string see <see cref="TableDataTypes.LoginStatusTokens"/></para>
        /// </summary>
        [SqlTableMember("varchar(512)", MySqlDataFormatString = "\"{0}\"")]
        public string LoginStatusTokens;

        /// <summary>
        /// The email associated with the user
        /// </summary>
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string Email;

        /// <summary>
        /// The size of the RequestHistory Column in the Database. This must be the same size as the "varbinary" declaration
        /// of the RequestHistory field in this class
        /// </summary>
        public static int RequestHistoryBytesSize = 2048;

        /// <summary>
        /// Byte array containing the ANS encoded JSON string representing the user's request history
        /// </summary>
        [SqlTableMember("varbinary(2048)")]
        public byte[] RequestHistory;

        /// <summary>
        /// The database id of the current user. This overrides the default Id field inheirited from the
        /// <see cref="MySqlTableDataMember{T}"/> class
        /// </summary>
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
                AuthTestString = (byte[])AuthTestString.Clone(),
                LoginStatusTokens = LoginStatusTokens,
                Email = Email,
                RequestHistory = RequestHistory
            };
            return ret;
        }

        /// <summary>
        /// Updates the user's settings by changing the value of the setting with the specified key to be the specified value
        /// </summary>
        /// <param name="key">The key of the setting to update</param>
        /// <param name="value">The value the setting should have after the update</param>
        /// <returns>True if the setting was found and updated successfully, false otherwise</returns>
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

        /// <summary>
        /// Decodes the requests in this user's RequestHistory and returns them
        /// </summary>
        /// <returns>A list containing all PreviousUserRequests stored in the user's RequestHistory</returns>
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

        /// <summary>
        /// Encodes the List of PreviousUserRequest objects, and stores them in this user's RequestHistory
        /// </summary>
        /// <param name="requestsIn">List of <see cref="PreviousUserRequest"/> objects that should become the current user's RequestHistory</param>
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

        /// <summary>
        /// <para>Deserializes the current user's data from the MySqlDataReader passed in</para>
        /// Overrides <see cref="MySqlTableDataMember{T}.Deserialize"/> due to the byte[] objects present in this class's fields
        /// </summary>
        /// <param name="reader">The MySqlDataReader object that contains the data to deserialize</param>
        /// <remarks>This method will break if <seealso cref="MySqlDataReader.Read"/> has not been called yet or if one of the 
        /// fields is of the value <seealso cref="DBNull"/></remarks>
        public override void Deserialize(MySqlDataReader reader)
        {
            AccessLevel = (int)reader["AccessLevel"];
            DerivedSecurityToken = (byte[])reader["DerivedSecurityToken"];
            SecurityQuestion = (string)reader["SecurityQuestion"];
            PersonalData = (byte[])reader["PersonalData"];
            Settings = (string)reader["Settings"];
            Company = (int)reader["Company"];
            AuthTestString = (byte[])reader["AuthTestString"];
            LoginStatusTokens = (string)reader["LoginStatusTokens"];
            Email = (string)reader["Email"];
            RequestHistory = (byte[])reader["RequestHistory"];
            UserId = (int)reader["id"];
        }

        /// <summary>
        /// Serializes this object into a MySql command string that can be used to insert its data into the table specified
        /// </summary>
        /// <param name="tableName">The table to construct an insert command for</param>
        /// <returns>A MySql command string used to insert this objects data into the table specified by <paramref name="tableName"/></returns>
        public override string Serialize(string tableName)
        {
            StringBuilder retBuilder = new StringBuilder();
            retBuilder.Append("insert into ");
            retBuilder.Append(tableName);
            retBuilder.Append("(AccessLevel, DerivedSecurityToken, SecurityQuestion, PersonalData, Settings, Company, AuthTestString, LoginStatusTokens, Email, RequestHistory) Values (");
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
            retBuilder.Append(MysqlDataConvertingUtil.ConvertToHexString(AuthTestString));
            retBuilder.Append(",");
            retBuilder.Append("\"" + LoginStatusTokens + "\"");
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
            if (other.AuthTestString.Length != AuthTestString.Length)
                return false;
            if (other.DerivedSecurityToken.Length != DerivedSecurityToken.Length)
                return false;

            for(int i = 0; i < other.AuthTestString.Length; i++)
            {
                if (other.AuthTestString[i] != AuthTestString[i])
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
            return Company + AccessLevel + SecurityQuestion.GetHashCode() + LoginStatusTokens.GetHashCode();
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

using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace MechanicsAssistantServer.Data.MySql.TableDataTypes
{
    class OverallUser : ISqlSerializable
    {
        public static readonly TableDataManipulator<OverallUser> Manipulator = new TableDataManipulator<OverallUser>();

        public int AccessLevel { get; set; }
        public byte[] DerivedSecurityToken { get; set; }
        public string SecurityQuestion { get; set; }
        public byte[] PersonalData { get; set; }
        public string Settings { get; set; }
        public int Company { get; set; }
        public byte[] AuthToken { get; set; }
        public string LoggedTokens { get; set; }

        private static string ConvertToHexString(byte[] a)
        {
            StringBuilder ret = new StringBuilder("0x");
            foreach (byte b in a)
            {
                if (b < 16)
                    ret.Append("0" + Convert.ToString(b, 16));
                else
                    ret.Append(Convert.ToString(b, 16));
            }
            return ret.ToString();
        }

        public OverallUser()
        {

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
                LoggedTokens = LoggedTokens
            };
            return ret;
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
        }

        public string Serialize(string tableName)
        {
            StringBuilder retBuilder = new StringBuilder();
            retBuilder.Append("insert into ");
            retBuilder.Append(tableName);
            retBuilder.Append("(AccessLevel, DerivedSecurityToken, SecurityQuestion, PersonalData, Settings, Company, AuthToken, LoggedToken) Values (");
            retBuilder.Append(AccessLevel);
            retBuilder.Append(",");
            retBuilder.Append(ConvertToHexString(DerivedSecurityToken));
            retBuilder.Append(",");
            retBuilder.Append("\"" + SecurityQuestion + "\"");
            retBuilder.Append(",");
            retBuilder.Append(ConvertToHexString(PersonalData));
            retBuilder.Append(",");
            retBuilder.Append("\"" + Settings + "\"");
            retBuilder.Append(",");
            retBuilder.Append(Company);
            retBuilder.Append(",");
            retBuilder.Append(ConvertToHexString(AuthToken));
            retBuilder.Append(",");
            retBuilder.Append("\"" + LoggedTokens + "\");");
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

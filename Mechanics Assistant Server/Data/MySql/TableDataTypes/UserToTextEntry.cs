using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    /// <summary>
    /// Class that represents a user's forum posting, and how that is stored in the MySql database
    /// </summary>
    public class UserToTextEntry : MySqlTableDataMember<UserToTextEntry>
    {
        public static readonly TableDataManipulator<UserToTextEntry> Manipulator = new TableDataManipulator<UserToTextEntry>();

        /// <summary>
        /// Text that the user posted to the forum
        /// </summary>
        [SqlTableMember("varchar(512)", MySqlDataFormatString = "\"{0}\"")]
        public string Text;

        /// <summary>
        /// Database id of the user that posted to the forum
        /// </summary>
        [SqlTableMember("int")]
        public int UserId;

        public UserToTextEntry()
        {

        }

        public UserToTextEntry(int userId, string text)
        {
            UserId = userId;
            Text = text;
        }

        public override ISqlSerializable Copy()
        {
            return new UserToTextEntry(UserId, Text);
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var other = obj as UserToTextEntry;
            return other.UserId == UserId && other.Text.Equals(Text);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Text.GetHashCode() + UserId;
        }


        public override string ToString()
        {
            return UserId + ": " + (Text ?? "");
        }

        protected override void ApplyDefaults()
        {
        }
    }
}

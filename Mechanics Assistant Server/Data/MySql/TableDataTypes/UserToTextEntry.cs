using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    public class UserToTextEntry : MySqlTableDataMember<UserToTextEntry>
    {
        public static readonly TableDataManipulator<UserToTextEntry> Manipulator = new TableDataManipulator<UserToTextEntry>();

        [SqlTableMember("varchar(512)", MySqlDataFormatString = "\"{0}\"")]
        public string Text;

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

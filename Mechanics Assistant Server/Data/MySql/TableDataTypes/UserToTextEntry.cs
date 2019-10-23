using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace MechanicsAssistantServer.Data.MySql.TableDataTypes
{
    class UserToTextEntry : ISqlSerializable
    {
        public static readonly TableDataManipulator<UserToTextEntry> Manipulator = new TableDataManipulator<UserToTextEntry>();

        public string Text { get; set; }
        public int UserId { get; set; }

        public int Id { get; set; }

        public UserToTextEntry()
        {

        }

        public UserToTextEntry(int userId, string text)
        {
            UserId = userId;
            Text = text;
        }

        public ISqlSerializable Copy()
        {
            return new UserToTextEntry(UserId, Text);
        }

        public void Deserialize(MySqlDataReader reader)
        {
            Text = (string)reader["MappedText"];
            UserId = (int)reader["UserId"];
            Id = (int)reader["id"];
        }

        public string Serialize(string tableName)
        {
            return "insert into " + tableName + "(MappedText, UserId) values (\"" + Text + "\", " + UserId + ");";
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
    }
}

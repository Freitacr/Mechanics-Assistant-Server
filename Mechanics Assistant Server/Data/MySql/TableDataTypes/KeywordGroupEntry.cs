using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    public class KeywordGroupEntry : ISqlSerializable
    {
        public static readonly TableDataManipulator<KeywordGroupEntry> Manipulator = new TableDataManipulator<KeywordGroupEntry>();
        public string GroupDefinition { get; set; }

        public int Id { get; set; }

        public KeywordGroupEntry()
        {

        }

        public KeywordGroupEntry(string groupDefinition)
        {
            GroupDefinition = groupDefinition;
        }

        public ISqlSerializable Copy()
        {
            return new KeywordGroupEntry(GroupDefinition);
        }

        public void Deserialize(MySqlDataReader reader)
        {
            GroupDefinition = (string)reader["GroupDefinition"];
            Id = (int)reader["id"];
        }

        public string Serialize(string tableName)
        {
            return "insert into " + tableName + "(GroupDefinition) values (\"" + GroupDefinition + "\");";
        }

        
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return (obj as KeywordGroupEntry).GroupDefinition.Equals(GroupDefinition);
        }

        public override int GetHashCode()
        {
            return GroupDefinition.GetHashCode();
        }
    }
}

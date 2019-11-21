using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    public class KeywordGroupEntry : MySqlTableDataMember<KeywordGroupEntry>
    {
        public static readonly TableDataManipulator<KeywordGroupEntry> Manipulator = new TableDataManipulator<KeywordGroupEntry>();
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string GroupDefinition;

        public KeywordGroupEntry()
        {

        }

        public KeywordGroupEntry(string groupDefinition)
        {
            GroupDefinition = groupDefinition;
        }

        public override ISqlSerializable Copy()
        {
            return new KeywordGroupEntry(GroupDefinition);
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

        protected override void ApplyDefaults() {}

        public override string ToString()
        {
            return GroupDefinition ?? "";
        }
    }
}

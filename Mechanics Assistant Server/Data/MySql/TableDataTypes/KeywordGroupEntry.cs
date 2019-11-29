using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    /// <summary>
    /// Class that represents a keyword grouping in the MySQL database
    /// </summary>
    public class KeywordGroupEntry : MySqlTableDataMember<KeywordGroupEntry>
    {
        public static readonly TableDataManipulator<KeywordGroupEntry> Manipulator = new TableDataManipulator<KeywordGroupEntry>();
        
        /// <summary>
        /// Definition of the keyword group (a space separated string of keywords)
        /// </summary>
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string GroupDefinition;

        /// <summary>
        /// Default Constructor, required by <see cref="TableDataManipulator{T}"/>
        /// </summary>
        public KeywordGroupEntry()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="groupDefinition">Definition of the keyword group (a space separated string of keywords)</param>
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

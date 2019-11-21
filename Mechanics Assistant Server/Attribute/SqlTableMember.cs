using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SqlTableMember : System.Attribute
    {
        public string MySqlDataType;
        public string MySqlDataTypeFlags = null;
        /// <summary>
        /// format string to insert the toString result from the object into
        /// If this is null, then the toString result from the field is used raw
        /// An example: "{0}", which would be used for string typed fields
        /// </summary>
        public string MySqlDataFormatString = null;
        public SqlTableMember(string mySqlDataType)
        {
            MySqlDataType = mySqlDataType;
        }
    }
}

using System;
using OldManInTheShopServer.Data.MySql;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Attribute
{
    /// <summary>
    /// <para>Attribute used to mark a field or property as being a column in a corresponding MySql Table in the database</para>
    /// <para>For information how this marking is used, see <see cref="MySqlTableDataMember{T}"/></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SqlTableMember : System.Attribute
    {
        /// <summary>
        /// <para>MySql data type that the column corresponding to the marked field or attribute should be</para>
        /// <para>A few examples would be "varchar(256)", "float", "int"</para>
        /// </summary>
        /// <remarks>The reason why this is a required field is that the name of fields in the backend of C# almost never
        /// line up with the names of the same data types in MySql
        /// 
        /// For example: the data type "int" in C#'s name is actually "Int32" in most scenarios, which is not a valid
        /// data type for MySql tables</remarks>
        public string MySqlDataType;

        /// <summary>
        /// <para>String containing any additional flags that should be added to the column corresponding to the field or property
        /// in the MySql Table</para>
        /// <para>An example of this would be "primary key auto_increment"</para>
        /// </summary>
        public string MySqlDataTypeFlags = null;

        /// <summary>
        /// <para>format string to insert the toString result from the object into
        /// If this is null, then the toString result from the field is used raw</para>
        /// An example: \"{0}\", which would be used for string typed fields, so the string literal gets wrapped in double quotes
        /// </summary>
        public string MySqlDataFormatString = null;

        /// <summary>
        /// <para>Attribute used to mark a field or property as being a column in a corresponding MySql Table in the database</para>
        /// <para>For information how this marking is used, see <see cref="MySqlTableDataMember{T}"/></para>
        /// </summary>
        /// <param name="mySqlDataType">
        /// <para>MySql data type that the column corresponding to the marked field or attribute should be</para>
        /// <para>A few examples would be "varchar(256)", "float", "int"</para>
        /// </param>
        public SqlTableMember(string mySqlDataType)
        {
            MySqlDataType = mySqlDataType;
        }
    }
}

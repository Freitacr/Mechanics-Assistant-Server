using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;
using System.Runtime.Serialization;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Data.MySql
{
    public class FieldMapping
    {
        public FieldInfo Field;
        public SqlTableMember Attribute;
    }

    [DataContract]
    public abstract class MySqlTableDataMember<T> : ISqlSerializable where T : class
    {
        [SqlTableMember("int", MySqlDataTypeFlags ="primary key auto_increment")]
        [DataMember]
        public int Id;
        protected static List<FieldMapping> Fields = null;

        public string GetCreateTableString(string tableName)
        {
            if (Fields == null)
            {
                DetermineFields();
            }
            StringBuilder retBuilder = new StringBuilder();
            retBuilder.Append("create table ");
            retBuilder.Append(tableName);
            retBuilder.Append(" (");
            List<string> fields = new List<string>();
            foreach (FieldMapping f in Fields)
            {
                StringBuilder fieldBuilder = new StringBuilder();
                fieldBuilder.Append(f.Field.Name);
                fieldBuilder.Append(" " + f.Attribute.MySqlDataType);
                if(f.Attribute.MySqlDataTypeFlags != null)
                {
                    fieldBuilder.Append(" " + f.Attribute.MySqlDataTypeFlags);
                }
                fields.Add(fieldBuilder.ToString());
            }
            retBuilder.Append(string.Join(", ", fields));
            retBuilder.Append(");");
            return retBuilder.ToString();
        }

        public virtual void Deserialize(MySqlDataReader reader)
        {
            if(Fields == null)
            {
                DetermineFields();
            }
            ApplyDefaults();
            foreach(FieldMapping f in Fields)
            {
                object dbRet = reader[f.Field.Name];
                if (dbRet == null || dbRet.GetType() == typeof(DBNull))
                    continue;
                try
                {
                    f.Field.SetValue(this, dbRet);
                } catch(TargetException)
                {
                    Logger.GetLogger(Logger.LoggerDefaultFileLocations.DEFAULT)
                        .Log(Logger.LogLevel.WARNING, "Invalid casting of type " + dbRet.GetType().FullName + " to " + f.Field.FieldType.FullName);
                }
            }
        }

        public virtual string Serialize(string tableName)
        {
            if (Fields == null)
            {
                DetermineFields();
            }
            StringBuilder retBuilder = new StringBuilder();
            retBuilder.Append("insert into ");
            retBuilder.Append(tableName);
            retBuilder.Append(" (");
            List<string> values = new List<string>();
            List<string> fields = new List<string>();
            foreach(FieldMapping f in Fields)
            {
                if (f.Field.Name.Equals("Id"))
                    continue;
                object val = f.Field.GetValue(this);
                if (f.Field.GetValue(this).Equals(GetDefaultForType(f.Field.FieldType)))
                    continue;
                fields.Add(f.Field.Name);
                string fieldVal = f.Field.GetValue(this).ToString().Replace("\"", "\\\"");
                if (f.Attribute.MySqlDataFormatString == null)
                    values.Add(fieldVal);
                else
                    values.Add(string.Format(f.Attribute.MySqlDataFormatString, fieldVal));
            }
            retBuilder.Append(string.Join(", ", fields));
            retBuilder.Append(") values (");
            retBuilder.Append(string.Join(", ", values));
            retBuilder.Append(");");
            return retBuilder.ToString();
        }

        private void DetermineFields()
        {
            Type thisType = GetType();
            var fields = thisType.GetFields();
            Fields = new List<FieldMapping>();
            foreach(FieldInfo f in fields)
            {
                foreach(CustomAttributeData d in f.CustomAttributes)
                {
                    if (d.AttributeType.Equals(typeof(SqlTableMember)))
                    {
                        Fields.Add(new FieldMapping() { Attribute = (SqlTableMember)System.Attribute.GetCustomAttribute(f, typeof(SqlTableMember)), Field = f });
                        break;
                    }
                }
            }
        }

        private object GetDefaultForType(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);
            return null;
        }

        protected abstract void ApplyDefaults();

        public abstract override bool Equals(object obj);

        public abstract override int GetHashCode();

        public abstract override string ToString();

        public abstract ISqlSerializable Copy();
    }
}

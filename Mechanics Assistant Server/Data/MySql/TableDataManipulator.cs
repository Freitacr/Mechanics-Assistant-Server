using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace OldManinTheShopServer.Data.MySql
{
    class TableDataManipulator<T> where T : ISqlSerializable, new()
    {
        public MySqlException LastException { get; private set; }

        public List<T> RetrieveDataWhere(MySqlConnection connection, string tableName, string where)
        {
            string commandString = "select * from " + tableName + " where " + where + ";";
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = commandString;
            MySqlDataReader reader;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (MySqlException e)
            {
                LastException = e;
                return null;
            }
            List<T> ret = new List<T>();
            while (reader.Read())
            {
                T toAdd = new T();
                toAdd.Deserialize(reader);
                ret.Add(toAdd);
            }
            reader.Close();
            return ret;
        }

        public T RetrieveDataWithId(MySqlConnection connection, string tableName, string id)
        {
            string commandString = "select * from " + tableName + " where id = " + id + ";";
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = commandString;
            MySqlDataReader reader;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (MySqlException e)
            {
                LastException = e;
                return default; //So this means return the default value of T? Which in our case would be null I am assuming
            }
            T ret = new T();
            if (!reader.Read())
            {
                LastException = null;
                return default;
            }
            ret.Deserialize(reader);
            reader.Close();
            return ret;
        }

        public List<T> RetrieveDataFrom(MySqlConnection connection, string tableName)
        {
            string commandString = "select * from " + tableName + ";";
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = commandString;
            MySqlDataReader reader;
            try
            {
                reader = cmd.ExecuteReader();
            } catch (MySqlException e)
            {
                LastException = e;
                return null;
            }
            List<T> ret = new List<T>();
            while (reader.Read())
            {
                T toAdd = new T();
                toAdd.Deserialize(reader);
                ret.Add(toAdd);
            }
            reader.Close();
            return ret;
        }

        public int InsertDataInto(MySqlConnection connection, string tableName, T toInsert)
        {
            string commandString = toInsert.Serialize(tableName);
            return ExecuteNonQuery(connection, commandString);
        }

        public int RemoveDataWithId(MySqlConnection connection, string tableName, int id)
        {
            string commandString = "delete from " + tableName + " where id = " + id + ";";
            return ExecuteNonQuery(connection, commandString);
        }


        public int RemoveDataWhere(MySqlConnection connection, string tableName, string where)
        {
            string commandString = "delete from " + tableName + " where " + where + ";";
            return ExecuteNonQuery(connection, commandString);
        }
        private int ExecuteNonQuery(MySqlConnection connection, string commandString)
        {
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = commandString;
            try
            {
                int ret = cmd.ExecuteNonQuery();
                return ret;
            }
            catch (MySqlException e)
            {
                LastException = e;
                return -1;
            }
        }
    }
}

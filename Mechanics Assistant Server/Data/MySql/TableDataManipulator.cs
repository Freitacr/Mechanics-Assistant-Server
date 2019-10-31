using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace OldManInTheShopServer.Data.MySql
{
    /// <summary>
    /// Class with the responsibility of performing basic database operations using the methods
    /// that come with the ISqlSerializable interface
    /// </summary>
    /// <typeparam name="T">A type that derives itself from the ISqlSerializable interface that has a default constructor</typeparam>
    public class TableDataManipulator<T> where T : ISqlSerializable, new()
    {
        /// <summary>
        /// The last MySqlException that occurred in this object
        /// </summary>
        public MySqlException LastException { get; private set; }

        /// <summary>
        /// Retrieves a List of the generic type object from the database that match the where conditional
        /// </summary>
        /// <param name="connection">Connection to the database to use when performing the request</param>
        /// <param name="tableName">Name of the table to retrieve from</param>
        /// <param name="where">Where conditional string that the generic objects must match</param>
        /// <returns>A list of the generic objects that match the where conditional, or null if an error occurs</returns>
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

        /// <summary>
        /// Retrieves a List of the generic type object from the database that match the where conditional
        /// </summary>
        /// <param name="connection">Connection to the database to use when performing the request</param>
        /// <param name="tableName">Name of the table to retrieve from</param>
        /// <param name="id">id of the object to retrieve</param>
        /// <returns>The generic object that has the specified id, or null if an object returns</returns>
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

        /// <summary>
        /// Attempts to retrieve all instances of the generic object from the specified table in the database using the specified conneciton
        /// </summary>
        /// <param name="connection">Connection to the database to use when performing the request</param>
        /// <param name="tableName">Name of the table to retrieve from</param>
        /// <returns>A list of the generic objects that are in the specified table, or null if an error occurred</returns>
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

        /// <summary>
        /// Attempts to insert the generic object into the table specified by <paramref name="tableName"/>
        /// </summary>
        /// <param name="connection">Connection to the database to use when performing the request</param>
        /// <param name="tableName">Name of the table to insert into</param>
        /// <param name="toInsert">The generic object to insert</param>
        /// <returns>The number of rows affected by the change. Will either be 1 or -1, where -1 occurs when an error does</returns>
        public int InsertDataInto(MySqlConnection connection, string tableName, T toInsert)
        {
            string commandString = toInsert.Serialize(tableName);
            return ExecuteNonQuery(connection, commandString);
        }

        /// <summary>
        /// Attempts to remove the item with the specified id from the table
        /// </summary>
        /// <param name="connection">Connection to the database to use when performing the request</param>
        /// <param name="tableName">Name of the table to remove from</param>
        /// <param name="id">The id of the object to remove</param>
        /// <returns>The number of rows affected by the change. Will either be 1 or -1, where -1 occurs when an error does</returns>
        public int RemoveDataWithId(MySqlConnection connection, string tableName, int id)
        {
            string commandString = "delete from " + tableName + " where id = " + id + ";";
            return ExecuteNonQuery(connection, commandString);
        }

        /// <summary>
        /// Attempts to remove all objects from the table specified by <paramref name="tableName"/> that match the where conditional
        /// </summary>
        /// <param name="connection">Connection to the database to use when performing the request</param>
        /// <param name="tableName">Name of the table to remove from</param>
        /// <param name="where">The where conditional that the data must match to be removed</param>
        /// <returns>The number of rows affected, or -1 if an error occurs</returns>
        public int RemoveDataWhere(MySqlConnection connection, string tableName, string where)
        {
            string commandString = "delete from " + tableName + " where " + where + ";";
            return ExecuteNonQuery(connection, commandString);
        }

        /// <summary>
        /// Attempts to execute the command specified by <paramref name="commandString"/>
        /// </summary>
        /// <param name="connection">Connection to the database to use when performing the request</param>
        /// <param name="commandString">String representing the MySql command to execute</param>
        /// <returns>The number of rows that were affected by the command, or -1 if an error occured</returns>
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

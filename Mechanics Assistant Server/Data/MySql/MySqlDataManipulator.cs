using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;
using MechanicsAssistantServer.Data.MySql.TableDataTypes;

#if DEBUG
[assembly: InternalsVisibleTo("Mechanics Assistant Server Tests")]
#endif

namespace MechanicsAssistantServer.Data.MySql
{
    /**
     * <summary>Class that holds the responsibility of manipulating the data
     * in the MySQL database in a standardized and easy to use way</summary>
     */
    class MySqlDataManipulator
    {
        /**<summary>Stores the last MySqlException encountered</summary>*/
        public MySqlException LastException { get; private set; }
        private MySqlConnection Connection;
        public static MySqlDataManipulator GlobalConfiguration = new MySqlDataManipulator();

        public MySqlDataManipulator()
        {
            
        }

        /**
         * <summary>Connects the MySqlDataManipulator object to the database</summary>
         * <param name="connectionString">The MySql connection string to use for connection</param>
         * <returns>true if connection was successful, false if an exception was encountered</returns>
         * <seealso cref="LastException"/>
         */
        public bool Connect(string connectionString)
        {
            try
            {
                Connection = new MySqlConnection();
                Connection.ConnectionString = connectionString;
                Connection.Open();
            } catch (MySqlException e)
            {
                LastException = e;
                return false;
            }
            return true;
        }

        public bool Close()
        {
            try
            {
                Connection.Close();
            } catch (MySqlException e)
            {
                LastException = e;
                return false;
            }
            return true;
        }

        public string GetConnectionString()
        {
            return Connection.ConnectionString;
        }

        public OverallUser GetUserById(int id)
        {
            OverallUser ret = OverallUser.Manipulator.RetrieveDataWithId(Connection, "overall_user_table", id.ToString());
            if(ret == null)
            {
                LastException = OverallUser.Manipulator.LastException;
            }
            return ret;
        }

        /**
         * <summary>Adds the user to the database using the data provided</summary>
         * <param name="email">The email of the user to create</param>
         * <param name="password">The user's password</param>
         * <param name="securityAnswer">The answer to the user's custom security question</param>
         * <param name="securityQuestion">The user's custom security question</param>
         * <returns>true if connection was successful, false if an exception was encountered</returns>
         * <seealso cref="LastException"/>
         */
        public bool AddUser(string email, string password, string securityQuestion, string securityAnswer)
        {
            throw new NotImplementedException();
        }

        /**
         * <summary>Adds the repair data to the database</summary>
         * <param name="companyId">The id of the company to add the part data to</param>
         * <param name="entryToAdd">The make of the machinery in question.</param>
         * <returns>true if insertion was successful, false if an exception was encountered</returns>
         * <seealso cref="LastException"/>
         */
        public bool AddDataEntry(int companyId, JobDataEntry entryToAdd)
        {
            try
            {
                JobDataEntry.Manipulator.InsertDataInto(Connection, "company" + companyId + "_non_validated_data", entryToAdd);
            } catch (MySqlException e)
            {
                LastException = e;
                return false;
            }
            return true;
        }

        /**
         * <summary>Adds the forum posting to the database</summary>
         * <param name="companyId">The id of the company to add the part data to</param>
         * <param name="userId">The userId of the user who made the post</param>
         * <param name="containedText">The actual posting that the user supplied. Should already be input scrubbed to prevent XSS</param>
         * <returns>true if connection was successful, false if an exception was encountered</returns>
         * <seealso cref="LastException"/>
         */
        public bool AddForumPosting(int companyId, int userId, string containedText)
        {
            throw new NotImplementedException();
        }

        /**
         * <summary>Adds the part data to the database</summary>
         * <param name="companyId">The id of the company to add the part data to</param>
         * <param name="make">The make of the machinery in question.</param>
         * <param name="model">The model of the machinery. May be equal to the make if not applicable</param>
         * <param name="partId">The part's id</param>
         * <param name="partName">The common name for the part</param>
         * <param name="year">The year of the machinery the data is for. If unknown, the value should be -1</param>
         * <returns>true if connection was successful, false if an exception was encountered</returns>
         * <seealso cref="LastException"/>
         */
        public bool AddPartCatalogueEntry(int companyId, string make, string model, string partId, string partName, int year=-1)
        {
            throw new NotImplementedException();
        }

        /**
         * <summary>Validates whether the database is in the correct format to be worked with by this class</summary>
         * <param name="createIfMissing">Flag for whether to create missing tables or the database itself if it is found to be missing</param>
         * <param name="databaseName">The name of the database to verify integrity of</param>
         */
        public bool ValidateDatabaseIntegrity(string databaseName, bool createIfMissing=true)
        {
            throw new NotImplementedException();
        }
    }
}

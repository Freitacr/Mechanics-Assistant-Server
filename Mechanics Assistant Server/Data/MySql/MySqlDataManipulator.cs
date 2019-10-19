using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;
using System.Runtime.Serialization.Json;
using MechanicsAssistantServer.Data.MySql.TableDataTypes;
using MechanicsAssistantServer.Util;
using OMISSecLib;

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

        private bool ExecuteNonQuery(MySqlCommand cmd)
        {
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                LastException = e;
                return false;
            }
            return true;
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
            OverallUser ret = OverallUser.Manipulator.RetrieveDataWithId(Connection, TableNameStorage.OverallUserTable, id.ToString());
            if(ret == null)
            {
                LastException = OverallUser.Manipulator.LastException;
            }
            return ret;
        }

        public List<OverallUser> GetUsersWhere(string where)
        {
            List<OverallUser> ret = OverallUser.Manipulator.RetrieveDataWhere(Connection, TableNameStorage.OverallUserTable, where);
            if(ret == null)
            {
                LastException = OverallUser.Manipulator.LastException;
            }
            return ret;
        }

        public bool UpdateUsersLoginToken(OverallUser toUpdate, LoggedTokens update)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LoggedTokens));
            MemoryStream writer = new MemoryStream();
            serializer.WriteObject(writer, update);
            byte[] tokensData = writer.ToArray();
            string tokens = Encoding.UTF8.GetString(tokensData);
            tokens = tokens.Replace("\"", "\\\"");
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "update " + TableNameStorage.OverallUserTable + " set LoggedToken=\"" + tokens + "\" where Email = \"" + toUpdate.Email + "\";";
            int res;
            try
            {
                res =cmd.ExecuteNonQuery();
            } catch (MySqlException e)
            {
                LastException = e;
                return false;
            }
            return res == 1;
        }

        public bool UpdateUsersSettings(OverallUser toUpdate)
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "update " + TableNameStorage.OverallUserTable + " set Settings=\"" + toUpdate.Settings.Replace("\"", "\\\"") + "\" where " +
                "id = " + toUpdate.UserId + ";";
            int res;
            try
            {
                res = cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                LastException = e;
                return false;
            }
            return res == 1;
        }

        public bool DeleteUserJobData(OverallUser toUpdate, int jobDataId)
        {
            if(jobDataId == 1)
            {
                toUpdate.Job1Id = toUpdate.Job2Id;
                toUpdate.Job1Results = toUpdate.Job2Results;
            }
            toUpdate.Job2Id = "";
            toUpdate.Job2Results = null;
            var cmd = Connection.CreateCommand();
            StringBuilder cmdBuilder = new StringBuilder("update " + TableNameStorage.OverallUserTable);
            cmdBuilder.Append(" set Job1Id=\"");
            cmdBuilder.Append(toUpdate.Job1Id);
            cmdBuilder.Append("\", Job1Results=");
            cmdBuilder.Append(MysqlDataConvertingUtil.ConvertToHexString(toUpdate.Job1Results));
            cmdBuilder.Append(", Job2Id=\"");
            cmdBuilder.Append(toUpdate.Job2Id);
            cmdBuilder.Append("\", Job2Results=");
            cmdBuilder.Append(MysqlDataConvertingUtil.ConvertToHexString(toUpdate.Job2Results));
            cmdBuilder.Append(" where id=" + toUpdate.UserId + ";");
            cmd.CommandText = cmdBuilder.ToString();
            int res;
            try
            {
                res = cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                LastException = e;
                return false;
            }
            return res == 1;
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
            SecuritySchemaLib secLib = new SecuritySchemaLib(SHA512.Create().ComputeHash);
            OverallUser toAdd = new OverallUser
            {
                Email = email,
                SecurityQuestion = securityQuestion,
                DerivedSecurityToken = secLib.ConstructDerivedSecurityToken(Encoding.UTF8.GetBytes(email), Encoding.UTF8.GetBytes(password)),
                AccessLevel = 1,
                Company = 1,
                Job1Id = "",
                Job2Id = ""
            };
            LoggedTokens defaultTokens = new LoggedTokens();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LoggedTokens));
            var writer = new MemoryStream();
            serializer.WriteObject(writer, defaultTokens);
            writer.Close();
            toAdd.LoggedTokens = Encoding.UTF8.GetString(writer.ToArray());
            toAdd.LoggedTokens = toAdd.LoggedTokens.Replace("\"", "\\\"");
            toAdd.Settings = OverallUser.GenerateDefaultSettings().Replace("\"", "\\\"");
            byte[] key, iv;
            key = new byte[32];
            iv = new byte[16];
            var encKey = secLib.ConstructUserEncryptionKey(toAdd.DerivedSecurityToken, Encoding.UTF8.GetBytes(securityAnswer));
            for (int i = 0, j = 32; j < toAdd.DerivedSecurityToken.Length; i++, j++)
            {
                key[i] = encKey[i];
                if ((i&1) == 0)
                    iv[i>>1] = encKey[j];
            }
            Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            byte[] toEncode = Encoding.UTF8.GetBytes("pass");
            toAdd.AuthToken = aes.CreateEncryptor().TransformFinalBlock(toEncode, 0, toEncode.Length);
            aes.Dispose();
            toAdd.EncodeRequests(new List<PreviousUserRequest>());
            int rowsAffected = OverallUser.Manipulator.InsertDataInto(Connection, TableNameStorage.OverallUserTable, toAdd);
            if(rowsAffected == -1)
            {
                LastException = OverallUser.Manipulator.LastException;
                return false;
            }
            return true;
        }

        public JobDataEntry GetDataEntryById(int companyId, int repairEntryId, bool validated=true)
        {
            string tableName;
            if(validated)
                tableName = TableNameStorage.CompanyValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            else
                tableName = TableNameStorage.CompanyNonValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            JobDataEntry entry = JobDataEntry.Manipulator.RetrieveDataWithId(
                Connection, tableName, repairEntryId.ToString());
            if(entry == null)
            {
                LastException = JobDataEntry.Manipulator.LastException;
                return null;
            }
            return entry;
        }

        public bool UpdateDataEntryRequirements(int companyId, JobDataEntry entryToUpdate, bool validated=true)
        {
            string toWrite = entryToUpdate.Requirements.Replace("\"", "\\\"");
            string tableName;
            if (validated)
                tableName = TableNameStorage.CompanyValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            else
                tableName = TableNameStorage.CompanyNonValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "update " + tableName + " set Requirements=\"" + toWrite + "\" where id=" + entryToUpdate.Id + ";";
            return ExecuteNonQuery(cmd);
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
            entryToAdd.FillDefaultValues();
            int res;
            try
            {
                res = JobDataEntry.Manipulator.InsertDataInto(
                    Connection, 
                    TableNameStorage.CompanyNonValidatedRepairJobTable.Replace(
                        "(n)", 
                        companyId.ToString()
                        ), 
                    entryToAdd);
            } catch (MySqlException e)
            {
                LastException = e;
                return false;
            }
            if (res < 1)
            {
                LastException = JobDataEntry.Manipulator.LastException;
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

        public bool AddCompany(string companyLegalName)
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "insert into " + TableNameStorage.CompanyIdTable + "(LegalName) values(\"" + companyLegalName + "\");";
            if (!ExecuteNonQuery(cmd))
                return false;
            cmd.CommandText = "select max(id) from " + TableNameStorage.CompanyIdTable;
            int companyId;
            try
            {
                var reader = cmd.ExecuteReader();
                //if we're here than there cannot have been an exception, so there is something in the table to get an id from
                reader.Read();
                companyId = (int)reader[0];
                reader.Close();
            }
            catch (MySqlException e)
            {
                LastException = e;
                return false;
            }

            //As we as the developers are the only ones who will be adding companies, this method will not
            //do due diligence in restoring the database to its state prior to this method call.
            cmd.CommandText = "create table " +
                TableNameStorage.CompanyComplaintKeywordGroupsTable.Replace("(n)", companyId.ToString()) +
                TableCreationDataDeclarationStrings.GroupDefinitionTable;
            if (!ExecuteNonQuery(cmd))
                return false;

            cmd.CommandText = "create table " +
                TableNameStorage.CompanyProblemKeywordGroupsTable.Replace("(n)", companyId.ToString()) +
                TableCreationDataDeclarationStrings.GroupDefinitionTable;
            if (!ExecuteNonQuery(cmd))
                return false;

            cmd.CommandText = "create table " +
                TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString()) +
                TableCreationDataDeclarationStrings.CompanyJoinRequest;
            if (!ExecuteNonQuery(cmd))
                return false;

            cmd.CommandText = "create table " +
                TableNameStorage.CompanyNonValidatedRepairJobTable.Replace("(n)", companyId.ToString()) +
                TableCreationDataDeclarationStrings.JobDataEntryTable;
            if (!ExecuteNonQuery(cmd))
                return false;

            cmd.CommandText = "create table " +
                TableNameStorage.CompanyValidatedRepairJobTable.Replace("(n)", companyId.ToString()) +
                TableCreationDataDeclarationStrings.JobDataEntryTable;
            if (!ExecuteNonQuery(cmd))
                return false;

            cmd.CommandText = "create table " +
                TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString()) +
                TableCreationDataDeclarationStrings.PartCatalogueTable;
            if (!ExecuteNonQuery(cmd))
                return false;

            cmd.CommandText = "create table " +
                TableNameStorage.CompanyPartsListsRequestsTable.Replace("(n)", companyId.ToString()) +
                TableCreationDataDeclarationStrings.PartsListAdditionRequest;
            if (!ExecuteNonQuery(cmd))
                return false;

            cmd.CommandText = "create table " +
                TableNameStorage.CompanyPartsRequestTable.Replace("(n)", companyId.ToString()) +
                TableCreationDataDeclarationStrings.CompanyPartsRequest;
            if (!ExecuteNonQuery(cmd))
                return false;

            cmd.CommandText = "create table " +
                TableNameStorage.CompanySafetyRequestsTable.Replace("(n)", companyId.ToString()) +
                TableCreationDataDeclarationStrings.SafetyAdditionRequest;
            if (!ExecuteNonQuery(cmd))
                return false;

            cmd.CommandText = "create table " +
                TableNameStorage.CompanySettingsTable.Replace("(n)", companyId.ToString()) +
                TableCreationDataDeclarationStrings.CompanySettings;
            if (!ExecuteNonQuery(cmd))
                return false;

            return true;
        }

        public bool CreateTable(string tableName, string tableData)
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = ("create table " + tableName + tableData);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            } catch (MySqlException e)
            {
                LastException = e;
                return false;
            }
        }

        public bool CreateDatabase(string databaseName)
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = ("create schema " + databaseName);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (MySqlException e)
            {
                LastException = e;
                return false;
            }
        }

        /**
         * <summary>Validates whether the database is in the correct format to be worked with by this class</summary>
         * <param name="createIfMissing">Flag for whether to create missing tables or the database itself if it is found to be missing</param>
         * <param name="databaseName">The name of the database to verify integrity of</param>
         */
        public bool ValidateDatabaseIntegrity(string databaseName)
        {
            string connectionString = Connection.ConnectionString;
            int databaseKeyLoc = connectionString.IndexOf("database");
            int databaseValueEnd = connectionString.IndexOf(";", databaseKeyLoc);
            connectionString = connectionString.Remove(databaseKeyLoc, (databaseValueEnd - databaseKeyLoc)+1);
            if (!Connect(connectionString))
                return false;
            if(!CreateDatabase(databaseName))
            {
                if(LastException.Number != 1007)
                {
                    return false;
                }
            }
            try
            {
                Connection.ChangeDatabase(databaseName);
            } catch (MySqlException e)
            {
                LastException = e;
                return false;
            }
            if (!CreateTable(TableNameStorage.OverallUserTable, TableCreationDataDeclarationStrings.OverallUserTable))
            {
                if (LastException.Number != 1050)
                    return false;
            }
            if (!CreateTable(TableNameStorage.CompanyIdTable, TableCreationDataDeclarationStrings.CompanyIdTable))
            {
                if (LastException.Number != 1050)
                    return false;
            }

            return true;
        }
    }
}

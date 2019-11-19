using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Runtime.Serialization.Json;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Util;
using ANSEncodingLib;
using OMISSecLib;
using System.Runtime.CompilerServices;
using System.Security;

#if DEBUG
[assembly: InternalsVisibleTo("Mechanics Assistant Server Tests")]
#endif

namespace OldManInTheShopServer.Data.MySql
{
    
     
    /// <summary>
    /// Class that holds the responsibility of manipulating the data in a standardized and easy to use way
    /// </summary>
    public class MySqlDataManipulator : IDisposable
    {
        /**<summary>Stores the last MySqlException encountered</summary>*/
        public MySqlException LastException { get; private set; }
        private MySqlConnection Connection;
        public string ConnectionString { get; private set; }
        public SecureString ConnectionPassword { get; private set; }
        /// <summary>
        /// Global Instance that should remain closed after initial assignment, connection, and closing.
        /// It is responsible for storing the connection string for use by other MySqlDataManipulators
        /// </summary>
        public static MySqlDataManipulator GlobalConfiguration = new MySqlDataManipulator();


        /// <summary>
        /// Executes the command stored in cmd, and returns whether the call succeeded
        /// </summary>
        /// <param name="cmd">The MySqlCommand to execute</param>
        /// <returns>true if the command succeeded, false if there was an exception</returns>
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
                ConnectionString = Connection.ConnectionString;
                if (ConnectionPassword == null)
                {
                    var portions = connectionString.Split(';');
                    foreach (string portion in portions)
                    {
                        if (portion.ToLower().StartsWith("password"))
                        {
                            SecureString password = new SecureString();
                            var portionSplit = portion.Split("=")[1];
                            string plaintextPass = portionSplit;
                            foreach (char c in plaintextPass)
                                password.AppendChar(c);
                            ConnectionPassword = password;
                        }
                    }
                }
            } catch (MySqlException e)
            {
                LastException = e;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Closes the connection with the database
        /// </summary>
        /// <returns>true if the connection was closed, false if an error occurred</returns>
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
        
        /// <summary>
        /// Returns the connection string that is currently bound to this object's connection with the database
        /// </summary>
        /// <returns>See summary</returns>
        public string GetConnectionString()
        {
            if (ConnectionPassword == null)
                throw new ArgumentNullException("Connection has not occurred yet, so a connection string has not been generated");

            return ConnectionString + ";password="+ConnectionPassword.ConvertToString() + ";";
        }

        public long GetCountInTable(string tableName)
        {
            string commandText = "select count(id) from " + tableName + ";";
            MySqlCommand cmd = Connection.CreateCommand();
            cmd.CommandText = commandText;
            var reader = cmd.ExecuteReader();
            reader.Read();
            long count = (long)reader[0];
            reader.Close();
            return count;
        }

        /// <summary>
        /// Returns all Problem Group definitions from the company's storage
        /// </summary>
        /// <param name="companyId">The id of the company to retrieve problem groups for</param>
        /// <returns>List of KeywordGroupEntry objects representing the problem groups</returns>
        public List<KeywordGroupEntry> GetCompanyProblemGroups(int companyId)
        {
            string tableName = TableNameStorage.CompanyProblemKeywordGroupsTable.Replace
                ("(n)", companyId.ToString());
            var res = KeywordGroupEntry.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (res == null)
                LastException = KeywordGroupEntry.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Attempts to add all KeywordGroupEntries passed in to the Company's Problem Group Storage
        /// </summary>
        /// <param name="companyId">Id of the company to insert data for</param>
        /// <param name="entries">List of KeywordGroupEntries to upload into the database</param>
        /// <returns>True if the action was successful, false if there was an error</returns>
        public bool AddCompanyProblemGroups(int companyId, List<KeywordGroupEntry> entries)
        {
            string tableName = TableNameStorage.CompanyProblemKeywordGroupsTable.Replace
                ("(n)", companyId.ToString());
            foreach(KeywordGroupEntry entry in entries)
            {
                if(KeywordGroupEntry.Manipulator.InsertDataInto(Connection, tableName, entry) != 1)
                {
                    LastException = KeywordGroupEntry.Manipulator.LastException;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Deletes the company's current storage of problem groups from the database
        /// </summary>
        /// <param name="companyId">id of the company to remove the problem groups from</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public bool DeleteCompanyProblemGroups(int companyId)
        {
            string tableName = TableNameStorage.CompanyProblemKeywordGroupsTable.Replace
                ("(n)", companyId.ToString());
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "delete from " + tableName + " where id>0;";
            return ExecuteNonQuery(cmd);
        }

        /// <summary>
        /// Retrieves and returns a list of all Complaint Group definitions that are in the company's storage
        /// </summary>
        /// <param name="companyId">id of the company to retrieve the complaint groups for</param>
        /// <returns>List of KeywordGroupEntries that represent the complaint groups</returns>
        public List<KeywordGroupEntry> GetCompanyComplaintGroups(int companyId)
        {
            string tableName = TableNameStorage.CompanyComplaintKeywordGroupsTable.Replace
                ("(n)", companyId.ToString());
            var res = KeywordGroupEntry.Manipulator.RetrieveDataFrom(Connection, tableName);
            if(res == null)
                LastException = KeywordGroupEntry.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Attempts to add all KeywordGroupEntries passed in into the company's storage
        /// </summary>
        /// <param name="companyId">id of the company to upload complaint groups for</param>
        /// <param name="entries">list of KeywordGroupEntries to upload into the database</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public bool AddCompanyComplaintGroups(int companyId, List<KeywordGroupEntry> entries)
        {
            string tableName = TableNameStorage.CompanyComplaintKeywordGroupsTable.Replace
                ("(n)", companyId.ToString());
            foreach (KeywordGroupEntry entry in entries)
            {
                if (KeywordGroupEntry.Manipulator.InsertDataInto(Connection, tableName, entry) != 1)
                {
                    LastException = KeywordGroupEntry.Manipulator.LastException;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Deletes the current company's storage of complaint groups
        /// </summary>
        /// <param name="companyId">id of the company to delete the storage of</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public bool DeleteCompanyComplaintGroups(int companyId)
        {
            string tableName = TableNameStorage.CompanyComplaintKeywordGroupsTable.Replace
                ("(n)", companyId.ToString());
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "delete from " + tableName + " where id>0;";
            return ExecuteNonQuery(cmd);
        }

        /// <summary>
        /// Retrieves the current storage of settings for the company specified
        /// </summary>
        /// <param name="companyId">the id of the company to retrieve the settings for</param>
        /// <returns>A list of CompanySettingsEntries that represent the company's current settings</returns>
        public List<CompanySettingsEntry> GetCompanySettings(int companyId)
        {
            string tableName = TableNameStorage.CompanySettingsTable.Replace("(n)", companyId.ToString());
            var setting = CompanySettingsEntry.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (setting == null)
                LastException = CompanySettingsEntry.Manipulator.LastException;
            return setting;
        }

        /// <summary>
        /// Retrieves the setting in the company's storage by the id specified
        /// </summary>
        /// <param name="companyId">id of the company to retrieve the setting for</param>
        /// <param name="settingsId">database id of the settings entry to retrieve</param>
        /// <returns>The company settings object that was specified by settingsId or null</returns>
        public CompanySettingsEntry GetCompanySettingsById(int companyId, int settingsId)
        {
            string tableName = TableNameStorage.CompanySettingsTable.Replace("(n)", companyId.ToString());
            var setting = CompanySettingsEntry.Manipulator.RetrieveDataWithId(Connection, tableName, settingsId.ToString());
            if (setting == null)
                LastException = CompanySettingsEntry.Manipulator.LastException;
            return setting;
        }

        /// <summary>
        /// Retrieves the settings in the company's storage that match the where condition
        /// </summary>
        /// <param name="companyId">Id of the company to retrieve the settings for</param>
        /// <param name="where">The where condition to match the settings with. Must end with a semicolon.</param>
        /// <returns>List of CompanySettingsEntries that match the condition, or null if an error occurred</returns>
        public List<CompanySettingsEntry> GetCompanySettingsWhere(int companyId, string where)
        {
            string tableName = TableNameStorage.CompanySettingsTable.Replace("(n)", companyId.ToString());
            var settings = CompanySettingsEntry.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if(settings == null)
                LastException = CompanySettingsEntry.Manipulator.LastException;
            return settings;
        }

        /// <summary>
        /// Updates the CompanySettingsEntry in the database to match the CompanySettingsEntry passed in
        /// </summary>
        /// <param name="companyId">id of the company to update the setting for</param>
        /// <param name="toUpdate">CompanySettingsEntry to update</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public bool UpdateCompanySettings(int companyId, CompanySettingsEntry toUpdate)
        {
            string tableName = TableNameStorage.CompanySettingsTable.Replace("(n)", companyId.ToString());
            string cmdText = "update " + tableName + " set SettingKey=\"" + toUpdate.SettingKey + "\", SettingValue=\"" + toUpdate.SettingValue + "\" where id=" + toUpdate.Id+";";
            var cmd = Connection.CreateCommand();
            cmd.CommandText = cmdText;
            return ExecuteNonQuery(cmd);
        }

        public bool AddCompanySetting(int companyId, CompanySettingsEntry toAdd)
        {
            int res = CompanySettingsEntry.Manipulator.InsertDataInto(Connection, TableNameStorage.CompanySettingsTable.Replace("(n)", companyId.ToString()), toAdd);
            if(res != 1)
            {
                LastException = CompanySettingsEntry.Manipulator.LastException;
            }
            return res == 1;
        }

        /// <summary>
        /// Retrieves and returns the company's model's accuracy based on automated testings
        /// </summary>
        /// <param name="companyId">Id of the company to retrieve the accuracy for</param>
        /// <returns>the double floating point value representing the accuracy of the company's models</returns>
        public double GetCompanyAccuracy(int companyId)
        {
            var company = CompanyId.Manipulator.RetrieveDataWithId(Connection, TableNameStorage.CompanyIdTable, companyId.ToString());
            if (company == null)
                return -1;
            return company.ModelAccuracy;
        }

        /// <summary>
        /// Adds a join request to the company's pending join request storage
        /// </summary>
        /// <param name="companyId">id of the company to add the request to</param>
        /// <param name="userId">id of the user requesting to join the company</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public bool AddJoinRequest(int companyId, int userId)
        {
            var user = GetUserById(userId);
            if (user == null)
                return false;
            var requests = user.DecodeRequests();
            if (requests == null)
            {
                LastException = null;
                return false;
            }
            PreviousUserRequest req = new PreviousUserRequest();
            req.Request = new RequestString() { Company = companyId, Type = "Join" };
            req.Request.CalculateMD5();
            requests.Add(req);
            user.EncodeRequests(requests);
            while(user.RequestHistory.Length >= TableCreationDataDeclarationStrings.RequestHistoryBytesSize)
            {
                requests.RemoveAt(0);
                user.EncodeRequests(requests);
            }
            if (!UpdateUserPreviousRequests(user))
                return false;
            string tableName = TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString());
            var res = JoinRequest.Manipulator.InsertDataInto(Connection, tableName, new JoinRequest(userId));
            if (res == -1)
            {
                LastException = JoinRequest.Manipulator.LastException;
                user.EncodeRequests(requests);
                if (requests == null)
                    return false;
                requests.Remove(req);
                user.EncodeRequests(requests);
                UpdateUserPreviousRequests(user);
            }
            return res == 1;
        }
        
        /// <summary>
        /// Removes a join request from the company's pending join request storage, either by accepting it or denying it
        /// </summary>
        /// <param name="companyId">id of the company to remove the join request from</param>
        /// <param name="requestId">database id of the request to remove</param>
        /// <param name="accept">boolean flag of whether to accept the request or not.</param>
        /// <remarks>If the request is accepted, the user who made the request that is being accepted has their company switched
        /// to the company specified by companyId</remarks>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public bool RemoveJoinRequest(int companyId, int requestId, bool accept=false)
        {
            string tableName = TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString());
            var request = JoinRequest.Manipulator.RetrieveDataWithId(Connection, tableName, requestId.ToString());
            var user = GetUserById(request.UserId);
            if (user == null)
                return false;
            var requests = user.DecodeRequests();
            if (requests == null)
            {
                LastException = null;
                return false;
            }
            string status = accept ? "Accepted" : "Denied";
            PreviousUserRequest req = new PreviousUserRequest();
            req.Request = new RequestString() { Company = companyId, Type = "Join" };
            req.Request.CalculateMD5();
            foreach(PreviousUserRequest prevRequest in requests)
            {
                if(prevRequest.Request.MD5.Equals(req.Request.MD5))
                {
                    prevRequest.RequestStatus = status;
                    break;
                }
            }
            user.EncodeRequests(requests);
            while (user.RequestHistory.Length >= TableCreationDataDeclarationStrings.RequestHistoryBytesSize)
            {
                requests.RemoveAt(0);
                user.EncodeRequests(requests);
            }
            if (!UpdateUserPreviousRequests(user))
                return false;
            if(JoinRequest.Manipulator.RemoveDataWithId(Connection, tableName, requestId) != 1)
            {
                foreach (PreviousUserRequest prevRequest in requests)
                {
                    if (prevRequest.Request.MD5.Equals(req.Request.MD5))
                    {
                        prevRequest.RequestStatus = "";
                        break;
                    }
                }
                user.EncodeRequests(requests);
                while (user.RequestHistory.Length >= TableCreationDataDeclarationStrings.RequestHistoryBytesSize)
                {
                    requests.RemoveAt(0);
                    user.EncodeRequests(requests);
                }
                UpdateUserPreviousRequests(user);
                return false;
            }
            if(accept)
            {
                user.Company = companyId;
                return UpdateUserCompany(user);
            }
            return true;
        }

        /// <summary>
        /// Retrieves a list of all pending join requests for the company specified
        /// </summary>
        /// <param name="companyId">Id of the company to retrieve requests for</param>
        /// <returns>List of JoinRequest objects that represent all pending requests</returns>
        public List<JoinRequest> GetJoinRequests(int companyId)
        {
            string tableName = TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString());
            var requests = JoinRequest.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (requests == null)
                LastException = JoinRequest.Manipulator.LastException;
            return requests;
        }

        /// <summary>
        /// Retrieves a join request by its database id
        /// </summary>
        /// <param name="companyId">id of the company to retrieve the join request from storage</param>
        /// <param name="requestId">database id of the request to retrieve</param>
        /// <returns>A JoinRequest object representing the request specified, or null if an error occurred</returns>
        public JoinRequest GetJoinRequestById(int companyId, int requestId)
        {
            string tableName = TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString());
            var request = JoinRequest.Manipulator.RetrieveDataWithId(Connection, tableName, requestId.ToString());
            if (request == null)
                LastException = JoinRequest.Manipulator.LastException;
            return request;
        }


        /// <summary>
        /// Retrieves a list of pending join requests that match the where condition from the company's storage
        /// </summary>
        /// <param name="companyId">id of the company to retrieve the requests from</param>
        /// <param name="where">where condition to make sure the requests match. Must end with a semicolon</param>
        /// <returns>List of JoinRequest objects that represent the objects matching the where clause. Or null if an error occurred</returns>
        public List<JoinRequest> GetJoinRequestsWhere(int companyId, string where)
        {
            string tableName = TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString());
            var requests = JoinRequest.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if (requests == null)
                LastException = JoinRequest.Manipulator.LastException;
            return requests;
        }

        /// <summary>
        /// Retrieves a list of pending join requests that match the where the database id of the request is within the range specified by idStart and idEnd
        /// </summary>
        /// <param name="companyId">id of the company to retrieve the requests from</param>
        /// <param name="idStart">Start of the range the database ids of the join requests must be in</param>
        /// <param name="idEnd">End of the range the database ids of the join requests must be in; exclusive</param>
        /// <returns>List of JoinRequest objects that represent the requests in the range. Or null if an error occurred</returns>
        public List<JoinRequest> GetJoinRequestsByIdRange(int companyId, int idStart, int idEnd)
        {
            string tableName = TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString());
            var requests = JoinRequest.Manipulator.RetrieveDataWhere(Connection, tableName, " id >= " + idStart + " and id <" + idEnd + ";");
            if (requests == null)
                LastException = JoinRequest.Manipulator.LastException;
            return requests;
        }

        /// <summary>
        /// Adds a user's forum post to the forum specified by companyId and repairJobId
        /// </summary>
        /// <param name="companyId">Id of the company to add the forum post to</param>
        /// <param name="repairJobId">Id of the repair job. Used to find the forum to add the post to</param>
        /// <param name="userForumPost">The post to upload into the database</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public bool AddForumPost(int companyId, int repairJobId, UserToTextEntry userForumPost)
        {
            string tableName = TableNameStorage.CompanyForumTable.Replace("(n)", companyId.ToString());
            tableName = tableName.Replace("(m)", repairJobId.ToString());
            var res = UserToTextEntry.Manipulator.InsertDataInto(Connection, tableName, userForumPost);
            if (res == -1)
                LastException = UserToTextEntry.Manipulator.LastException;
            return res == 1;
        }

        /// <summary>
        /// Retrieves all forum posts from the forum specified by companyId and repairJobId
        /// </summary>
        /// <param name="companyId">id of the company who has the forum required</param>
        /// <param name="repairJobId">id of the RepairJobEntry the forum is about</param>
        /// <returns>A list of UserToTextEntries that represent all user posts currently in the forum, or null if an error occurred</returns>
        public List<UserToTextEntry> GetForumPosts(int companyId, int repairJobId)
        {
            string tableName = TableNameStorage.CompanyForumTable.Replace("(n)", companyId.ToString());
            tableName = tableName.Replace("(m)", repairJobId.ToString());
            var res = UserToTextEntry.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (res == null)
                LastException = UserToTextEntry.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrieves a forum post by its database id
        /// </summary>
        /// <param name="companyId">Id of the company who has the forum the post is in</param>
        /// <param name="repairJobId">Id of the RepairJobEntry the forum is about</param>
        /// <param name="forumPostId">Database id of the post to try and retrieve</param>
        /// <returns>A UserToTextEntry object that represents the forum post requested, or null if an error occurred</returns>
        public UserToTextEntry GetForumPost(int companyId, int repairJobId, int forumPostId)
        {
            string tableName = TableNameStorage.CompanyForumTable.Replace("(n)", companyId.ToString());
            tableName = tableName.Replace("(m)", repairJobId.ToString());
            var res = UserToTextEntry.Manipulator.RetrieveDataWithId(Connection, tableName, forumPostId.ToString());
            if (res == null)
                LastException = UserToTextEntry.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrives a list of forum posts matching the where conditional
        /// </summary>
        /// <param name="companyId">Id of the company that hosts the forum to retrieve the posts from</param>
        /// <param name="repairJobId">Id of the RepairJobEntry the forum is about</param>
        /// <param name="where">The condition the posts must match to be retrieved. Must end with a semicolon</param>
        /// <returns>A List of UserToTextEntries that represent all posts that match the where condition. Or null if an error occurred</returns>
        public List<UserToTextEntry> GetForumPostsWhere(int companyId, int repairJobId, string where)
        {
            string tableName = TableNameStorage.CompanyForumTable.Replace("(n)", companyId.ToString());
            tableName = tableName.Replace("(m)", repairJobId.ToString());
            var res = UserToTextEntry.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if (res == null)
                LastException = UserToTextEntry.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Attempts to remove a forum post from the specified company's storage
        /// </summary>
        /// <param name="companyId">database id of the company to remove the forum post from</param>
        /// <param name="repairJobId">database id of the repair job entry the forum post is about. Identifies the fourm</param>
        /// <param name="entry">Forum Post to remove</param>
        /// <returns>true if the removal is successful, false if an error occurs</returns>
        public bool RemoveForumPost(int companyId, int repairJobId, UserToTextEntry entry)
        {
            string tableName = TableNameStorage.CompanyForumTable.Replace("(n)", companyId.ToString());
            tableName = tableName.Replace("(m)", repairJobId.ToString());
            var res = UserToTextEntry.Manipulator.RemoveDataWithId(Connection, tableName, entry.Id);
            if (res == -1)
                LastException = UserToTextEntry.Manipulator.LastException;
            return res == 1;    
        }

        /// <summary>
        /// Attempts to update the PartCatalogueEntry stored in the database to match <paramref name="toUpdate"/>
        /// </summary>
        /// <param name="companyId">database id of the company to update the storage of</param>
        /// <param name="toUpdate">the PartCatalogueEntry that represents the new state of the PartCatalogueEntry</param>
        /// <returns>true if the update was successful, or false if an error occurs</returns>
        public bool UpdatePartEntry(int companyId, PartCatalogueEntry toUpdate)
        {
            StringBuilder commandTextBuilder = new StringBuilder();
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            commandTextBuilder.Append("update ");
            commandTextBuilder.Append(tableName);
            commandTextBuilder.Append(" set Make=\"");
            commandTextBuilder.Append(toUpdate.Make);
            commandTextBuilder.Append("\", Model=\"");
            commandTextBuilder.Append(toUpdate.Model);
            commandTextBuilder.Append("\", PartId=\"");
            commandTextBuilder.Append(toUpdate.PartId);
            commandTextBuilder.Append("\", PartName=\"");
            commandTextBuilder.Append(toUpdate.PartName);
            commandTextBuilder.Append("\", Year=");
            commandTextBuilder.Append(toUpdate.Year);
            commandTextBuilder.Append(" where id=" + toUpdate.Id);
            commandTextBuilder.Append(";");
            var cmd = Connection.CreateCommand();
            cmd.CommandText = commandTextBuilder.ToString();
            return ExecuteNonQuery(cmd);
        }

        /// <summary>
        /// Attempts to add the PartCatalogueEntry specified by <paramref name="toAdd"/> to the company's storage
        /// </summary>
        /// <param name="companyId">database id of the company to add the PartCatalogueEntry to</param>
        /// <param name="toAdd">The PartCatalogueEntry to add to the company's storage</param>
        /// <returns>true if the addition was successful, false if an error occurs</returns>
        public bool AddPartEntry(int companyId, PartCatalogueEntry toAdd)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = PartCatalogueEntry.Manipulator.InsertDataInto(Connection, tableName, toAdd);
            if (res == -1)
                LastException = PartCatalogueEntry.Manipulator.LastException;
            return res == 1;
        }

        /// <summary>
        /// Retrieves a list of all PartCatalogueEntry objects stored in the company's storage
        /// </summary>
        /// <param name="companyId">database id of the company to retrieve PartCatelogueEntry objects from</param>
        /// <returns>A list of all PartCatalogueEntry objects in the company's storage, or null if an error occurs</returns>
        public List<PartCatalogueEntry> GetPartCatalogueEntries(int companyId)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = PartCatalogueEntry.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (res == null)
                LastException = PartCatalogueEntry.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrieves the PartCatelogueEntry with the specified id from the company's storage
        /// </summary>
        /// <param name="companyId">database id of the company to retrieve the PartCatalogueEntry from</param>
        /// <param name="entryId">database id of the part catelogue entry to retrieve</param>
        /// <returns>The PartCatelogueEntry with the specified database id, or null if an error occurred</returns>
        public PartCatalogueEntry GetPartCatalogueEntryById(int companyId, int entryId)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = PartCatalogueEntry.Manipulator.RetrieveDataWithId(Connection, tableName, entryId.ToString());
            if (res == null)
                LastException = PartCatalogueEntry.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrieves a list of PartCatelogueEntry objects that match the <paramref name="where"/> conditional string privided
        /// </summary>
        /// <param name="companyId">id of the company to retrieve the catelogue entries from</param>
        /// <param name="where">conditional string the part catelogue entries must match to be returned. Must end with a semicolon</param>
        /// <returns>A list of PartCatelogueEntry objects that match the where conditional string, or null if an error occurred</returns>
        public List<PartCatalogueEntry> GetPartCatalogueEntriesWhere(int companyId, string where)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = PartCatalogueEntry.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if (res == null)
                LastException = PartCatalogueEntry.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrieves a list of PartCatelogueEntry objects that have a database id within the range of <paramref name="startId"/> to <paramref name="endId"/>
        /// </summary>
        /// <param name="companyId">Database id of the company to retrive the PartCatalogueEntries from</param>
        /// <param name="startId">Starting database id to begin the allowed range; inclusive</param>
        /// <param name="endId">Ending database id to end the allowed range; exclusive</param>
        /// <returns>List of PartCatelogueEntry objects that have database ids within the specified range</returns>
        public List<PartCatalogueEntry> GetPartCatalogueEntriesByIdRange(int companyId, int startId, int endId)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = PartCatalogueEntry.Manipulator.RetrieveDataWhere(Connection, tableName, " id >= " + startId + " and id <" + endId + ";");
            if (res == null)
                LastException = PartCatalogueEntry.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Attempts to remove the part catalogue entry specified by <paramref name="entryId"/>
        /// </summary>
        /// <param name="companyId">database id of the company to remove the part catelogue entry from</param>
        /// <param name="entryId">database id of the part catelogue entry to remove</param>
        /// <returns>true if the entry was removed successfully or false if an error occurs</returns>
        public bool RemovePartCatalogueEntry(int companyId, int entryId)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = PartCatalogueEntry.Manipulator.RemoveDataWithId(Connection, tableName, entryId);
            if (res == -1)
                LastException = PartCatalogueEntry.Manipulator.LastException;
            return res == 1;
        }

        /// <summary>
        /// Attempts to update the parts list addition request represented by <paramref name="request"/> in the database
        /// </summary>
        /// <param name="companyId">Database id of the company to update the storage of</param>
        /// <param name="request">RequirementAdditionRequest that represents the new state of the parts list addition request</param>
        /// <returns>true if the update was successful, false if an error occurred</returns>
        public bool UpdatePartsListAdditionRequest(int companyId, RequirementAdditionRequest request)
        {
            string tableName = TableNameStorage.CompanyPartsListsRequestsTable.Replace("(n)", companyId.ToString());
            string updateString = "update " + tableName + " set RequestedAdditions=\"" + request.RequestedAdditions + "\" where id=" + request.Id + ";";
            var cmd = Connection.CreateCommand();
            cmd.CommandText = updateString;
            return ExecuteNonQuery(cmd);
        }

        /// <summary>
        /// Attempts to add the parts list addition request represented by <paramref name="request"/> to the company's storage
        /// </summary>
        /// <param name="companyId">Database id of the company to add the request to</param>
        /// <param name="request">RequirementAdditionRequest representing the parts list addition requst to add</param>
        /// <returns>True if the addition was successful, false if an error occurred</returns>
        public bool AddPartsListAdditionRequest(int companyId, RequirementAdditionRequest request)
        {
            var user = GetUserById(request.UserId);
            if (user == null)
                return false;
            var requests = user.DecodeRequests();
            if (requests == null)
            {
                LastException = null;
                return false;
            }
            PreviousUserRequest req = new PreviousUserRequest();
            req.Request = new RequestString() { Company = companyId, Type = "PartsList" };
            req.Request.CalculateMD5(request.RequestedAdditions+request.ValidatedDataId);
            requests.Add(req);
            user.EncodeRequests(requests);
            while (user.RequestHistory.Length >= TableCreationDataDeclarationStrings.RequestHistoryBytesSize)
            {
                requests.RemoveAt(0);
                user.EncodeRequests(requests);
            }
            if (!UpdateUserPreviousRequests(user))
                return false;
            string tableName = TableNameStorage.CompanyPartsListsRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.InsertDataInto(Connection, tableName, request);
            if (res == -1)
            {
                LastException = RequirementAdditionRequest.Manipulator.LastException;
                user.EncodeRequests(requests);
                if (requests == null)
                    return false;
                requests.Remove(req);
                user.EncodeRequests(requests);
                UpdateUserPreviousRequests(user);
            }
            return res == 1;
        }

        /// <summary>
        /// Attempts to remove the parts list addition request with the specified id from the company's storage by accepting or denying its changes
        /// </summary>
        /// <param name="companyId">Database id of the company to remove the request from</param>
        /// <param name="requestId">Database id of the request to remove</param>
        /// <param name="accept">Whether to accept the request being removed</param>
        /// <remarks>If the request is accepted, then the user is notified and the RepairJobEntry the request is proposing changes to
        /// will have its requirements updated. 
        /// If it is denied then the user will be notified, but the RepairJobEntry will not change
        /// </remarks>
        /// <returns>True if the removal was successful, false if an error occurred</returns>
        public bool RemovePartsListAdditionRequest(int companyId, int requestId, bool accept=false)
        {
            string tableName = TableNameStorage.CompanyPartsListsRequestsTable.Replace("(n)", companyId.ToString());
            var request = RequirementAdditionRequest.Manipulator.RetrieveDataWithId(Connection, tableName, requestId.ToString());
            var user = GetUserById(request.UserId);
            if (user == null)
                return false;
            var requests = user.DecodeRequests();
            if (requests == null)
            {
                LastException = null;
                return false;
            }
            string status = accept ? "Accepted" : "Denied";
            PreviousUserRequest req = new PreviousUserRequest();
            req.Request = new RequestString() { Company = companyId, Type = "PartsList" };
            req.Request.CalculateMD5(request.RequestedAdditions + request.ValidatedDataId);
            foreach (PreviousUserRequest prevRequest in requests)
            {
                if (prevRequest.Request.MD5.Equals(req.Request.MD5))
                {
                    prevRequest.RequestStatus = status;
                    break;
                }
            }
            user.EncodeRequests(requests);
            while (user.RequestHistory.Length >= TableCreationDataDeclarationStrings.RequestHistoryBytesSize)
            {
                requests.RemoveAt(0);
                user.EncodeRequests(requests);
            }
            if (!UpdateUserPreviousRequests(user))
                return false;
            if (JoinRequest.Manipulator.RemoveDataWithId(Connection, tableName, requestId) != 1)
            {
                foreach (PreviousUserRequest prevRequest in requests)
                {
                    if (prevRequest.Request.MD5.Equals(req.Request.MD5))
                    {
                        prevRequest.RequestStatus = "";
                        break;
                    }
                }
                user.EncodeRequests(requests);
                while (user.RequestHistory.Length >= TableCreationDataDeclarationStrings.RequestHistoryBytesSize)
                {
                    requests.RemoveAt(0);
                    user.EncodeRequests(requests);
                }
                UpdateUserPreviousRequests(user);
                return false;
            }

            if(accept)
            {
                var jobDataEntry = GetDataEntryById(companyId, request.ValidatedDataId);
                if (jobDataEntry == null)
                    return true;
                RequirementsEntry requirements = RequirementsEntry.ParseJsonString(jobDataEntry.Requirements);
                List<int> toAdd = JsonDataObjectUtil<List<int>>.ParseObject(request.RequestedAdditions);
                foreach (int i in toAdd)
                    if (!requirements.Parts.Contains(i))
                        requirements.Parts.Add(i);
                jobDataEntry.Requirements = requirements.GenerateJsonString();
                return UpdateDataEntryRequirements(companyId, jobDataEntry);
            }

            return true;
        }

        /// <summary>
        /// Retrieves a list of RequirementAdditionRequest objects that represent all parts list addition requests in the specified company's storage
        /// </summary>
        /// <param name="companyId">Database id of the company to retrieve the parts list addition requests from</param>
        /// <returns>List of RequirementAdditionRequest objects that represent all of the parts list addition requests in the company's storage,
        /// or null if an error occurs</returns>
        public List<RequirementAdditionRequest> GetPartsListAdditionRequests(int companyId)
        {
            string tableName = TableNameStorage.CompanyPartsListsRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrieves the RequirementAdditionRequest object that represents the parts list addition request with the specified database id
        /// </summary>
        /// <param name="companyId">Database id of the company to retrieve the parts list addition request from</param>
        /// <param name="requirementId">Database id of the parts list addition request</param>
        /// <returns>The RequirementAdditionRequest object that represents the parts list addition request with the specified id</returns>
        public RequirementAdditionRequest GetPartsListAdditionRequestById(int companyId, int requirementId)
        {
            string tableName = TableNameStorage.CompanyPartsListsRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataWithId(Connection, tableName, requirementId.ToString());
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrieves a list of RequirementAdditionRequest objects representing 
        /// the parts list addition requests from the specified company's storage matching the where conditional string
        /// </summary>
        /// <param name="companyId">database id of the company who's storage should be searched</param>
        /// <param name="where">The where conditional string the parts list addition requests must match. Must end with a semicolon</param>
        /// <returns>A list of RequirementAdditionRequest objects representing the matching parts list addition requests, 
        /// or null if an error occurred</returns>
        public List<RequirementAdditionRequest> GetPartsListAdditionRequestsWhere(int companyId, string where)
        {
            string tableName = TableNameStorage.CompanyPartsListsRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrieves a list of RequirementAdditionRequest objects from the specified company's parts list addition request storage 
        /// that have a database id in the range specified by <paramref name="startId"/> to <paramref name="endId"/>
        /// </summary>
        /// <param name="companyId">Database id of the company to retrieve the RequirementAdditionRequest objects from</param>
        /// <param name="startId">Starting id of the range of database ids to find; inclusive</param>
        /// <param name="endId">Ending id of the range of database ids to find; exclusive</param>
        /// <returns>A list of RequirementAdditionRequest objects representing the matching parts list addition requests. Or null if an error occurred</returns>
        public List<RequirementAdditionRequest> GetPartsListAdditionRequestsByIdRange(int companyId, int startId, int endId)
        {
            string tableName = TableNameStorage.CompanyPartsListsRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataWhere(Connection, tableName, " id >= " + startId + " and id <" + endId + ";");
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Updates the safety addition request represented by the RequirementAdditionRequest passed in to match the RequirementAdditionRequest passed in
        /// </summary>
        /// <param name="companyId">Database id of the company to update the storage of</param>
        /// <param name="request">The RequirementAdditionRequest that represents the new state of the safety addition request in the database</param>
        /// <returns>true if the update was successful, false if an error occurred</returns>
        public bool UpdateSafetyAdditionRequest(int companyId, RequirementAdditionRequest request)
        {
            string tableName = TableNameStorage.CompanySafetyRequestsTable.Replace("(n)", companyId.ToString());
            string updateString = "update " + tableName + " set RequestedAdditions=\"" + request.RequestedAdditions.Replace("\"", "\\\"") + "\" where id=" + request.Id + ";";
            var cmd = Connection.CreateCommand();
            cmd.CommandText = updateString;
            return ExecuteNonQuery(cmd);
        }

        /// <summary>
        /// Adds the RequirementAdditionRequest to the specified company's safety requirement addition request storage
        /// </summary>
        /// <param name="companyId">Database id of the company to add the request to</param>
        /// <param name="request">Requset to add</param>
        /// <returns>True if the addition was successful, false if an error occurred</returns>
        public bool AddSafetyAdditionRequest(int companyId, RequirementAdditionRequest request)
        {
            var user = GetUserById(request.UserId);
            if (user == null)
                return false;
            var requests = user.DecodeRequests();
            if (requests == null)
            {
                LastException = null;
                return false;
            }
            PreviousUserRequest req = new PreviousUserRequest();
            req.Request = new RequestString() { Company = companyId, Type = "Safety" };
            req.Request.CalculateMD5(request.RequestedAdditions+request.ValidatedDataId);
            requests.Add(req);
            user.EncodeRequests(requests);
            while (user.RequestHistory.Length >= TableCreationDataDeclarationStrings.RequestHistoryBytesSize)
            {
                requests.RemoveAt(0);
                user.EncodeRequests(requests);
            }
            if (!UpdateUserPreviousRequests(user))
                return false;
            string tableName = TableNameStorage.CompanySafetyRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.InsertDataInto(Connection, tableName, request);
            if (res == -1)
            {
                LastException = RequirementAdditionRequest.Manipulator.LastException;
                user.EncodeRequests(requests);
                if (requests == null)
                    return false;
                requests.Remove(req);
                user.EncodeRequests(requests);
                UpdateUserPreviousRequests(user);
            }
            return res == 1;
        }

        /// <summary>
        /// Attempts to remove the safety addition request from the specified company's safety addition request storage by accepting or denying it
        /// </summary>
        /// <param name="companyId">Database id of the company to remove the request from</param>
        /// <param name="requestId">Database id of the request to remove</param>
        /// <param name="accept">Whether to accept the changes proposed by the request</param>
        /// <remarks>If the request is accepted, the user is notified and the JobDataEntry the request is proposing changes to
        /// will be have its safety requirements updated
        /// if it is not accpeted, the user will simply be notified</remarks>
        /// <returns>True if the removal was successful, or false if an error occurred</returns>
        public bool RemoveSafetyAdditionRequest(int companyId, int requestId, bool accept = false)
        {
            string tableName = TableNameStorage.CompanySafetyRequestsTable.Replace("(n)", companyId.ToString());
            var request = RequirementAdditionRequest.Manipulator.RetrieveDataWithId(Connection, tableName, requestId.ToString());
            var user = GetUserById(request.UserId);
            if (user == null)
                return false;
            var requests = user.DecodeRequests();
            if (requests == null)
            {
                LastException = null;
                return false;
            }
            string status = accept ? "Accepted" : "Denied";
            PreviousUserRequest req = new PreviousUserRequest();
            req.Request = new RequestString() { Company = companyId, Type = "Safety" };
            req.Request.CalculateMD5(request.RequestedAdditions + request.ValidatedDataId);
            foreach (PreviousUserRequest prevRequest in requests)
            {
                if (prevRequest.Request.MD5.Equals(req.Request.MD5))
                {
                    prevRequest.RequestStatus = status;
                    break;
                }
            }
            user.EncodeRequests(requests);
            while (user.RequestHistory.Length >= TableCreationDataDeclarationStrings.RequestHistoryBytesSize)
            {
                requests.RemoveAt(0);
                user.EncodeRequests(requests);
            }
            if (!UpdateUserPreviousRequests(user))
                return false;
            if (JoinRequest.Manipulator.RemoveDataWithId(Connection, tableName, requestId) != 1)
            {
                foreach (PreviousUserRequest prevRequest in requests)
                {
                    if (prevRequest.Request.MD5.Equals(req.Request.MD5))
                    {
                        prevRequest.RequestStatus = "";
                        break;
                    }
                }
                user.EncodeRequests(requests);
                while (user.RequestHistory.Length >= TableCreationDataDeclarationStrings.RequestHistoryBytesSize)
                {
                    requests.RemoveAt(0);
                    user.EncodeRequests(requests);
                }
                UpdateUserPreviousRequests(user);
                return false;
            }

            if (accept)
            {
                var jobDataEntry = GetDataEntryById(companyId, request.ValidatedDataId);
                if (jobDataEntry == null)
                    return true;
                RequirementsEntry requirements = RequirementsEntry.ParseJsonString(jobDataEntry.Requirements);
                    if (!requirements.Safety.Contains(request.RequestedAdditions))
                        requirements.Safety.Add(request.RequestedAdditions);
                jobDataEntry.Requirements = requirements.GenerateJsonString();
                return UpdateDataEntryRequirements(companyId, jobDataEntry);
            }


            return true;
        }

        /// <summary>
        /// Retrieves all RequirementAdditionRequests from the specified company's safety addition request storage
        /// </summary>
        /// <param name="companyId">Database id of the company to search the safety addition request storage of</param>
        /// <returns>A list of all RequirementAdditionRequests representing the contents of the company's safety addition request storage, or null if an error occurred</returns>
        public List<RequirementAdditionRequest> GetSafetyAdditionRequests(int companyId)
        {
            string tableName = TableNameStorage.CompanySafetyRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrieves the RequirementAdditionRequest with the specified id from the company's safety addition request storage
        /// </summary>
        /// <param name="companyId">The id of the company to search the storage of</param>
        /// <param name="requirementId">The id of the safety addition request to find</param>
        /// <returns>The RequirementAdditionRequest representing the safety addition request with the specified id, or null if an error occurred</returns>
        public RequirementAdditionRequest GetSafetyAdditionRequestById(int companyId, int requirementId)
        {
            string tableName = TableNameStorage.CompanySafetyRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataWithId(Connection, tableName, requirementId.ToString());
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrieves a list of RequirementAdditionRequest objects that match the where condition from the company's safety addition request storage
        /// </summary>
        /// <param name="companyId">Database Id of the company to retrieve the requests from</param>
        /// <param name="where">Where conditional string that the safety addition requests must match. Must end with a semicolon</param>
        /// <returns>A list of RequirementAdditionRequest objects from the company's safety addition request storage matching the conditional. Or null if an error occurred</returns>
        public List<RequirementAdditionRequest> GetSafetyAdditionRequestsWhere(int companyId, string where)
        {
            string tableName = TableNameStorage.CompanySafetyRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrieves a list of SafetyAdditionRequest objects who's database id are within the specified range
        /// </summary>
        /// <param name="companyId">Id of the company to retrieve the requests from</param>
        /// <param name="startId">Starting id of the range to search for. Inclusive</param>
        /// <param name="endId">Ending id of the range to search for. Exclusive</param>
        /// <returns>A list of RequirementAdditionRequest objects that are within the specified id range, or null if an error occurred</returns>
        public List<RequirementAdditionRequest> GetSafetyAdditionRequestsByIdRange(int companyId, int startId, int endId)
        {
            string tableName = TableNameStorage.CompanySafetyRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataWhere(Connection, tableName, " id >= " + startId + " and id <" + endId + ";");
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Attempts to add the specified PartsRequest object to the specified company's storage
        /// </summary>
        /// <param name="companyId">Id of the company to add the PartsRequest to</param>
        /// <param name="request">The request to add</param>
        /// <returns>True if the adding was successful, or false if an error occurred</returns>
        public bool AddPartsRequest(int companyId, PartsRequest request)
        {
            var user = GetUserById(request.UserId);
            if (user == null)
                return false;
            var requests = user.DecodeRequests();
            if (requests == null)
            {
                LastException = null;
                return false;
            }
            PreviousUserRequest req = new PreviousUserRequest();
            req.Request = new RequestString() { Company = companyId, Type = "Parts" };
            req.Request.CalculateMD5(request.ReferencedParts + request.JobId);
            requests.Add(req);
            user.EncodeRequests(requests);
            while (user.RequestHistory.Length >= TableCreationDataDeclarationStrings.RequestHistoryBytesSize)
            {
                requests.RemoveAt(0);
                user.EncodeRequests(requests);
            }
            if (!UpdateUserPreviousRequests(user))
                return false;
            string tableName = TableNameStorage.CompanyPartsRequestTable.Replace("(n)", companyId.ToString());
            var res = PartsRequest.Manipulator.InsertDataInto(Connection, tableName, request);
            if (res == -1)
                LastException = PartsRequest.Manipulator.LastException;
            return res == 1;
        }

        /// <summary>
        /// Attempts to remove the PartsRequest with the specified id, either by accepting or denying the request
        /// </summary>
        /// <param name="companyId">Database id of the company to remove the request from</param>
        /// <param name="requestId">Database id of the request to accept or deny</param>
        /// <param name="accept">Whether to accept the request or deny it</param>
        /// <remarks>Whether the request is accepted or denied has no further effect on our database beyond informing the user of the decision</remarks>
        /// <returns>True if the removal was successful, or false if an error occurred</returns>
        public bool RemovePartsRequest(int companyId, int requestId, bool accept=false)
        {
            string tableName = TableNameStorage.CompanyPartsRequestTable.Replace("(n)", companyId.ToString());
            var request = PartsRequest.Manipulator.RetrieveDataWithId(Connection, tableName, requestId.ToString());
            var user = GetUserById(request.UserId);
            if (user == null)
                return false;
            var requests = user.DecodeRequests();
            if (requests == null)
            {
                LastException = null;
                return false;
            }
            string status = accept ? "Accepted" : "Denied";
            PreviousUserRequest req = new PreviousUserRequest();
            req.Request = new RequestString() { Company = companyId, Type = "Parts" };
            req.Request.CalculateMD5(request.ReferencedParts + request.JobId);
            foreach (PreviousUserRequest prevRequest in requests)
            {
                if (prevRequest.Request.MD5.Equals(req.Request.MD5))
                {
                    prevRequest.RequestStatus = status;
                    break;
                }
            }
            user.EncodeRequests(requests);
            while (user.RequestHistory.Length >= TableCreationDataDeclarationStrings.RequestHistoryBytesSize)
            {
                requests.RemoveAt(0);
                user.EncodeRequests(requests);
            }
            if (!UpdateUserPreviousRequests(user))
                return false;
            if (JoinRequest.Manipulator.RemoveDataWithId(Connection, tableName, requestId) != 1)
            {
                foreach (PreviousUserRequest prevRequest in requests)
                {
                    if (prevRequest.Request.MD5.Equals(req.Request.MD5))
                    {
                        prevRequest.RequestStatus = "";
                        break;
                    }
                }
                user.EncodeRequests(requests);
                while (user.RequestHistory.Length >= TableCreationDataDeclarationStrings.RequestHistoryBytesSize)
                {
                    requests.RemoveAt(0);
                    user.EncodeRequests(requests);
                }
                UpdateUserPreviousRequests(user);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves a list of all the PartsRequest objects that are in the company's storage
        /// </summary>
        /// <param name="companyId">The database id of the company to retrieve the PartsRequest objects from</param>
        /// <returns>A list of all the PartsRequest object that are in the specified company's storage, or null if an error occurred</returns>
        public List<PartsRequest> GetPartsRequests(int companyId)
        {
            string tableName = TableNameStorage.CompanyPartsRequestTable.Replace("(n)", companyId.ToString());
            var res = PartsRequest.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (res == null)
                LastException = PartsRequest.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrieves the PartsRequest object in the database with the specified id
        /// </summary>
        /// <param name="companyId">database id of the company to retrieve the PartsRequest from</param>
        /// <param name="requestId">database id of the PartsRequest object to retrieve</param>
        /// <returns>The PartsRequest object with the specified id, or null if an error occured</returns>
        public PartsRequest GetPartsRequestById(int companyId, int requestId)
        {
            string tableName = TableNameStorage.CompanyPartsRequestTable.Replace("(n)", companyId.ToString());
            var res = PartsRequest.Manipulator.RetrieveDataWithId(Connection, tableName, requestId.ToString());
            if (res == null)
                LastException = PartsRequest.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrieves a list of PartsRequest objects that match the where condition
        /// </summary>
        /// <param name="companyId">database id of the company to retrieve parts requests from</param>
        /// <param name="where">where conditional to match the PartsRequest objects with. Must end with a semicolon</param>
        /// <returns>A List of PartsRequest objects that match the where conditional, or null if an error occurred</returns>
        public List<PartsRequest> GetPartsRequestsWhere(int companyId, string where)
        {
            string tableName = TableNameStorage.CompanyPartsRequestTable.Replace("(n)", companyId.ToString());
            var res = PartsRequest.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if (res == null)
                LastException = PartsRequest.Manipulator.LastException;
            return res;
        }

        /// <summary>
        /// Retrieves a list of PartsRequest objects from the database that have ids in the specified range
        /// </summary>
        /// <param name="companyId">database id of the company to retrieve parts requests from</param>
        /// <param name="startId">starting id of the range to retrieve PartsRequest objects from. Inclusive</param>
        /// <param name="endId">ending id of the range to retrieve PartsRequest objects from. Exclusive</param>
        /// <returns>A list of PartsRequests objects that match have ids in the specified range, or null if an error occurred</returns>
        public List<PartsRequest> GetPartsRequestsByIdRange(int companyId, int startId, int endId)
        {
            string tableName = TableNameStorage.CompanyPartsRequestTable.Replace("(n)", companyId.ToString());
            var res = PartsRequest.Manipulator.RetrieveDataWhere(Connection, tableName, " id >= " + startId + " and id <" + endId + ";");
            if (res == null)
                LastException = PartsRequest.Manipulator.LastException;
            return res;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the database version of the user passed in's request history to match the history of the OverallUser passed in
        /// </summary>
        /// <param name="toUpdate">The OverallUser whose request history the database version should match</param>
        /// <returns>True if the update was successful, false otherwise</returns>
        public bool UpdateUserPreviousRequests(OverallUser toUpdate)
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "update " + TableNameStorage.OverallUserTable + " set RequestHistory=" + MysqlDataConvertingUtil.ConvertToHexString(toUpdate.RequestHistory) + " where id=" + toUpdate.UserId + ";";
            return ExecuteNonQuery(cmd);
        }

        /// <summary>
        /// Adds the encoded job results to the user specified
        /// </summary>
        /// <param name="toUpdate">The user object to update the database with</param>
        /// <param name="jobId">The Id of the job the mechanic worked requested information for (shop sided id, not our database id)</param>
        /// <param name="encodedJobResults">Encoded results of the query for ease of recall</param>
        /// <returns>True if the update occurred successfully, or false if an error occurred</returns>
        public bool AddJobDataToUser(OverallUser toUpdate, string jobId, byte[] encodedJobResults)
        {
            toUpdate.Job2Results = toUpdate.Job1Results;
            toUpdate.Job2Id = toUpdate.Job1Id;
            toUpdate.Job1Id = jobId;
            toUpdate.Job1Results = encodedJobResults;
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

        /// <summary>
        /// Retrives a the user by their database id
        /// </summary>
        /// <param name="id">database id of the user to retrieve</param>
        /// <returns>The OverallUser object representing the user in the database, or null if an error occurred</returns>
        public OverallUser GetUserById(int id)
        {
            OverallUser ret = OverallUser.Manipulator.RetrieveDataWithId(Connection, TableNameStorage.OverallUserTable, id.ToString());
            if(ret == null)
            {
                LastException = OverallUser.Manipulator.LastException;
            }
            return ret;
        }

        /// <summary>
        /// Retrieves a list of OverallUser objects from the database matching the where conditional
        /// </summary>
        /// <param name="where">The where condition that the OverallUser objects in the database must match to be retrieved. Must end with a semicolon</param>
        /// <returns>A list of OverallUser objects that match the where conditional, or null if an error occurred</returns>
        public List<OverallUser> GetUsersWhere(string where)
        {
            List<OverallUser> ret = OverallUser.Manipulator.RetrieveDataWhere(Connection, TableNameStorage.OverallUserTable, where);
            if(ret == null)
            {
                LastException = OverallUser.Manipulator.LastException;
            }
            return ret;
        }

        /// <summary>
        /// Updates the database version of the user's authentication and login tokens to match the ones provided by <paramref name="update"/>
        /// </summary>
        /// <param name="toUpdate">The user to modify, exclusively used to find the database entry to update</param>
        /// <param name="update">The login and authentication tokens to update</param>
        /// <returns>true if the operation occurred successfully, or false if an error occurred</returns>
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

        /// <summary>
        /// Updates the database version of the overall user's settings to match the one passed in. Matched by id
        /// </summary>
        /// <param name="toUpdate">User object used to update the database version's settings</param>
        /// <returns>True if the update occurred successfully, false otherwise</returns>
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

        /// <summary>
        /// Deletes the previous data on a job data query from the specified user
        /// </summary>
        /// <param name="toUpdate">The user to remove the job data query from</param>
        /// <param name="jobDataId">Id of the job data to remove, valid range is 1 to 2 inclusive</param>
        /// <returns>true if the deletion was successful, false otherwise</returns>
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
        public bool AddUser(string email, string password, string securityQuestion, string securityAnswer, int accessLevel = 1, int companyId=1)
        {
            SecuritySchemaLib secLib = new SecuritySchemaLib(SHA512.Create().ComputeHash);
            OverallUser toAdd = new OverallUser
            {
                Email = email,
                SecurityQuestion = securityQuestion,
                DerivedSecurityToken = secLib.ConstructDerivedSecurityToken(Encoding.UTF8.GetBytes(email), Encoding.UTF8.GetBytes(password)),
                AccessLevel = accessLevel,
                Company = companyId,
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

        /// <summary>
        /// Updates the database version of toUpdate's company to match the version passed in
        /// </summary>
        /// <param name="toUpdate">OverallUser object that contains the state the database version should reflect</param>
        /// <returns>true if the update was successful, false otherwise</returns>
        public bool UpdateUserCompany(OverallUser toUpdate)
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "update " + TableNameStorage.OverallUserTable + " set Company=" + toUpdate.Company + " where " +
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

        public bool UpdateUserAccessLevel(OverallUser toUpdate)
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "update " + TableNameStorage.OverallUserTable + " set AccessLevel=" + toUpdate.AccessLevel + " where " +
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

        /// <summary>
        /// Updates the storage location of the JobDataEntry specified, moving it into the validated data if it was not previously validated, or vice versa
        /// </summary>
        /// <param name="companyId">Id of the company to perform the update to</param>
        /// <param name="toSwitch">The JobDataEntry to switch the validation status of</param>
        /// <param name="wasValidated">Whether the JobDataEntry is currently in the validated data set</param>
        /// <returns>True if the swap was successful, false otherwise</returns>
        public bool UpdateValidationStatus(int companyId, JobDataEntry toSwitch, bool wasValidated)
        {
            string previousTableName;
            string newTableName;
            if(wasValidated)
            {
                previousTableName = TableNameStorage.CompanyValidatedRepairJobTable.Replace("(n)", companyId.ToString());
                newTableName = TableNameStorage.CompanyNonValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            } else
            {
                previousTableName = TableNameStorage.CompanyNonValidatedRepairJobTable.Replace("(n)", companyId.ToString());
                newTableName = TableNameStorage.CompanyValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            }
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "delete from " + previousTableName + " where id=" + toSwitch.Id + ";";
            if (!ExecuteNonQuery(cmd))
            {
                return false; //Deletion was unsuccessful, just stop
            }
            int res = JobDataEntry.Manipulator.InsertDataInto(Connection, newTableName, toSwitch);
            if(res != 1)
            {
                //Something went horribly wrong, so attempt insertion back into original table
                JobDataEntry.Manipulator.InsertDataInto(Connection, previousTableName, toSwitch);
                return false;
            }

            if(!wasValidated)
            {
                JobDataEntry added = GetDataEntriesWhere(companyId, " Complaint=\"" + toSwitch.Complaint + "\";", wasValidated).Where(entry => entry.Equals(toSwitch)).First();
                CreateTable(TableNameStorage.CompanyForumTable.Replace("(n)", companyId.ToString())
                    .Replace("(m)", added.Id.ToString()), TableCreationDataDeclarationStrings.UserForumEntryTable);
            } else
            {
                cmd.CommandText = "drop table " + TableNameStorage.CompanyForumTable.Replace("(n)", companyId.ToString())
                    .Replace("(m)", toSwitch.Id.ToString());
                LastException = null;
                if (!ExecuteNonQuery(cmd))
                {
                    if (LastException != null)
                        Logger.Global.Log(Logger.LogLevel.WARNING, "Forum Table " + companyId + "," + toSwitch.Id + " was not deleted successfully");
                }
            }
            return res == 1;
        }

        /// <summary>
        /// Retrieves a JobDataEntry by its database id
        /// </summary>
        /// <param name="companyId">Id of the company to retrieve the JobDataEntry from</param>
        /// <param name="repairEntryId">Database id of the JobDataEntry</param>
        /// <param name="validated">Whether to perform the search in the company's validated data set</param>
        /// <returns>A JobDataEntry object with the specified id, or null if an error occurred</returns>
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
        
        /// <summary>
        /// Retrieves a list of JobDataEntries that match the where conditional.
        /// </summary>
        /// <param name="companyId">Id of the company to retrieve JobDataEntries from</param>
        /// <param name="where">The conditional the JobDataEntries must match. Must end with a semicolon</param>
        /// <param name="validated">Whether the search should be performed in the company's validated data set</param>
        /// <returns>A list of JobDataEntires that match the where conditional, or null if an error occurred</returns>
        public List<JobDataEntry> GetDataEntriesWhere(int companyId, string where, bool validated=false)
        {
            string tableName;
            if(validated)
            {
                tableName = TableNameStorage.CompanyValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            } else
            {
                tableName = TableNameStorage.CompanyNonValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            }
            List<JobDataEntry> ret = JobDataEntry.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if (ret == null)
            {
                LastException = OverallUser.Manipulator.LastException;
            }
            return ret;
        }

        /// <summary>
        /// Updates the requirements of the JobDataEntry in the database with the one passed in, matched via ids
        /// </summary>
        /// <param name="companyId">Id of the company to perform the update on</param>
        /// <param name="entryToUpdate">The entry to update the database version with</param>
        /// <param name="validated">Whether the database entry to update exists in the company's validated data set or not</param>
        /// <returns>True if the update was successful, or false if an error occurred</returns>
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

        public bool UpdateDataEntryGroups(int companyId, JobDataEntry entryToUpdate, bool validated=true, bool complaint = true)
        {
            string toWrite;
            string fieldName;
            if (!complaint)
            {
                toWrite = entryToUpdate.ProblemGroups.Replace("\"", "\\\"");
                fieldName = "ProblemGroupings";
            }
            else
            {
                toWrite = entryToUpdate.ComplaintGroups.Replace("\"", "\\\"");
                fieldName = "ComplaintGroupings";
            }
            string tableName;
            if (validated)
                tableName = TableNameStorage.CompanyValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            else
                tableName = TableNameStorage.CompanyNonValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "update " + tableName + " set " + fieldName + "=\"" + toWrite + "\" where id=" + entryToUpdate.Id + ";";
            return ExecuteNonQuery(cmd);
        }

        public List<JobDataEntry> GetDataEntriesByProblemGroup(int companyId, int problemGroupId, bool validated=true)
        {
            string tableName;
            if (validated)
                tableName = TableNameStorage.CompanyValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            else
                tableName = TableNameStorage.CompanyNonValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            string where = " ProblemGroupings like \"%" + problemGroupId + "%\";";
            List<JobDataEntry> ret = GetDataEntriesWhere(companyId, where, validated);
            return ret;
        }

        /// <summary>
        /// Returns a list of JobDataEntries that are matched with the specified complaint group
        /// </summary>
        /// <param name="companyId">Id of the company to retrieve JobDataEntries from</param>
        /// <param name="complaintGroupId">Database id of complaint group the JobDataEntries are compared against</param>
        /// <param name="validated">Whether the JobDataEntries returned should be from the company's Validated data set</param>
        /// <returns>A list of JobDataEntries that match the specified complaint group, or null if an error occurs</returns>
        public List<JobDataEntry> GetDataEntriesByComplaintGroup(int companyId, int complaintGroupId, bool validated = true)
        {

            string tableName;
            if (validated)
                tableName = TableNameStorage.CompanyValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            else
                tableName = TableNameStorage.CompanyNonValidatedRepairJobTable.Replace("(n)", companyId.ToString());
            string where = " ComplaintGroupings like \"%"+ complaintGroupId+"%\";";
            List<JobDataEntry> ret = GetDataEntriesWhere(companyId, where, validated);
            return ret;
        }

        /**
         * <summary>Adds the repair data to the database</summary>
         * <param name="companyId">The id of the company to add the part data to</param>
         * <param name="entryToAdd">The make of the machinery in question.</param>
         * <returns>true if insertion was successful, false if an exception was encountered</returns>
         * <seealso cref="LastException"/>
         */
        public bool AddDataEntry(int companyId, JobDataEntry entryToAdd, bool validated=false)
        {
            string tableName;
            if (validated)
            {
                tableName = TableNameStorage.CompanyValidatedRepairJobTable.Replace(
                    "(n)",
                    companyId.ToString()
                    );
            }else
            {
                tableName = TableNameStorage.CompanyNonValidatedRepairJobTable.Replace(
                    "(n)",
                    companyId.ToString()
                    );
            }
            entryToAdd.FillDefaultValues();
            int res;
            try
            {
                res = JobDataEntry.Manipulator.InsertDataInto(
                    Connection, 
                    tableName, 
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

            if(validated)
            {
                JobDataEntry added = GetDataEntriesWhere(companyId, " Complaint=\"" + entryToAdd.Complaint + "\";", validated).Where(entry => entry.Equals(entryToAdd)).First();
                CreateTable(TableNameStorage.CompanyForumTable.Replace("(n)", companyId.ToString())
                    .Replace("(m)", added.Id.ToString()), TableCreationDataDeclarationStrings.UserForumEntryTable);
            }

            return true;
        }

        /// <summary>
        /// Attempts to add the company specified to the database, along with all the tables that are required to be set up for addition
        /// </summary>
        /// <param name="companyLegalName">Legal name of the company to add</param>
        /// <returns>true if company addition was successful, false otherwise</returns>
        public bool AddCompany(string companyLegalName)
        {
            DateTime dt = DateTime.Now;
            var cmd = Connection.CreateCommand();

            cmd.CommandText = "insert into " + TableNameStorage.CompanyIdTable + "(LegalName, ModelAccuracy, LastTrainedTime, LastValidatedTime) values(\"" + companyLegalName + "\", 0.0, date("+dt.ToString()+"), date("+dt.ToString()+");";
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

            string companySettingsTable = TableNameStorage.CompanySettingsTable.Replace("(n)", companyId.ToString());

            cmd.CommandText = "create table " +
                companySettingsTable +
                TableCreationDataDeclarationStrings.CompanySettings;
            if (!ExecuteNonQuery(cmd))
                return false;

            if (CompanySettingsEntry.Manipulator.InsertDataInto(Connection, companySettingsTable, new CompanySettingsEntry(CompanySettingsKey.Public, true.ToString())) != 1)
                return false;

            if (CompanySettingsEntry.Manipulator.InsertDataInto(Connection, companySettingsTable, new CompanySettingsEntry(CompanySettingsKey.Downvotes, 5.ToString())) != 1)
                return false;

            if (CompanySettingsEntry.Manipulator.InsertDataInto(Connection, companySettingsTable, new CompanySettingsEntry(CompanySettingsKey.RetrainInterval, 7.ToString())) != 1)
                return false;

            if (CompanySettingsEntry.Manipulator.InsertDataInto(Connection, companySettingsTable, new CompanySettingsEntry(CompanySettingsKey.ProblemPredictor, "Database")) != 1)
                return false;
            if (CompanySettingsEntry.Manipulator.InsertDataInto(Connection, companySettingsTable, new CompanySettingsEntry(CompanySettingsKey.KeywordClusterer, "Similarity")) != 1)
                return false;
            if (CompanySettingsEntry.Manipulator.InsertDataInto(Connection, companySettingsTable, new CompanySettingsEntry(CompanySettingsKey.KeywordPredictor, "Bayesian")) != 1)
                return false;

            return true;
        }
        
        public CompanyId GetCompanyById(int companyId)
        {
            CompanyId ret = CompanyId.Manipulator.RetrieveDataWithId(Connection, TableNameStorage.CompanyIdTable, companyId.ToString());
            if (ret == null)
                LastException = CompanyId.Manipulator.LastException;
            return ret;
        }

        public List<CompanyId> GetPublicCompanies()
        {
            string companyIdTable = TableNameStorage.CompanyIdTable;
            List<CompanyId> companies = CompanyId.Manipulator.RetrieveDataFrom(Connection, companyIdTable);
            if(companies == null)
            {
                LastException = CompanyId.Manipulator.LastException;
                return new List<CompanyId>();
            }
            return companies.Where(company => IsCompanyPublic(company)).ToList();
        }

        private bool IsCompanyPublic(CompanyId company)
        {
            string companySettingTable = TableNameStorage.CompanySettingsTable.Replace("(n)", company.Id.ToString());
            List<CompanySettingsEntry> companySettings = CompanySettingsEntry.Manipulator.RetrieveDataWhere(Connection, companySettingTable, "SettingKey=\"" + CompanySettingsKey.Public + "\"");
            if (companySettings == null || companySettings.Count == 0)
                return false;
            bool.TryParse(companySettings[0].SettingValue, out bool ret);
            return ret;
        }

        public bool UpdateCompanyAutomatedTestingResults(CompanyId companyIn)
        {
            string updateCommand = "update " + TableNameStorage.CompanyIdTable + " set ModelAccuracy=" + companyIn.ModelAccuracy + " where id=" + companyIn.Id + ";";
            MySqlCommand cmd = Connection.CreateCommand();
            cmd.CommandText = updateCommand;
            return ExecuteNonQuery(cmd);
        }

        public bool UpdateCompanyTrainingTime(CompanyId companyIn)
        {
            string updateCommand = "update " + TableNameStorage.CompanyIdTable + " set LastTrainedTime=\"" + companyIn.LastTrainedTime + "\" where id=" + companyIn.Id + ";";
            MySqlCommand cmd = Connection.CreateCommand();
            cmd.CommandText = updateCommand;
            return ExecuteNonQuery(cmd);
        }
        public List<CompanyId> GetCompaniesWithNamePortion(string namePortion)
        {
            List<CompanyId> ret = CompanyId.Manipulator.RetrieveDataWhere(Connection, TableNameStorage.CompanyIdTable, "LegalName like \"%" + namePortion + "%\"");
            if (ret == null)
                LastException = CompanyId.Manipulator.LastException;
            return ret;
        }

        /// <summary>
        /// Attempts to create a table in the current database with the specified name and data string
        /// </summary>
        /// <param name="tableName">Name of the table to create, usually comes from <code>TableNameStorage</code> or modified from the same class</param>
        /// <param name="tableData">Data string of the table to create, comes from <code>TableCreationDataDeclarationStrings</code></param>
        /// <returns>True if creation was successful, false otherwise</returns>
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

        /// <summary>
        /// Attempts to create a schema with the specified name
        /// </summary>
        /// <param name="databaseName">The name of the schema to create</param>
        /// <returns>true if creation was successful, false otherwise</returns>
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
            string connectionString = GetConnectionString();
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

        /// <summary>
        /// Disposes of the current Manipulator
        /// </summary>
        public void Dispose()
        {
            if(Connection != null)
            {
                if (Connection.State == System.Data.ConnectionState.Open)
                    Connection.Close();
            }
        }
    }
}
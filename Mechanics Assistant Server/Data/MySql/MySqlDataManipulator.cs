using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;
using System.Runtime.Serialization.Json;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Util;
using ANSEncodingLib;
using OMISSecLib;

#if DEBUG
[assembly: InternalsVisibleTo("Mechanics Assistant Server Tests")]
#endif

namespace OldManInTheShopServer.Data.MySql
{
    /**
     * <summary>Class that holds the responsibility of manipulating the data
     * in the MySQL database in a standardized and easy to use way</summary>
     */
    public class MySqlDataManipulator : IDisposable
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

        public List<KeywordGroupEntry> GetCompanyProblemGroups(int companyId)
        {
            string tableName = TableNameStorage.CompanyProblemKeywordGroupsTable.Replace
                ("(n)", companyId.ToString());
            var res = KeywordGroupEntry.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (res == null)
                LastException = KeywordGroupEntry.Manipulator.LastException;
            return res;
        }

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

        public bool DeleteCompanyProblemGroups(int companyId)
        {
            string tableName = TableNameStorage.CompanyProblemKeywordGroupsTable.Replace
                ("(n)", companyId.ToString());
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "delete from " + tableName + " where id>0;";
            return ExecuteNonQuery(cmd);
        }

        public List<KeywordGroupEntry> GetCompanyComplaintGroups(int companyId)
        {
            string tableName = TableNameStorage.CompanyComplaintKeywordGroupsTable.Replace
                ("(n)", companyId.ToString());
            var res = KeywordGroupEntry.Manipulator.RetrieveDataFrom(Connection, tableName);
            if(res == null)
                LastException = KeywordGroupEntry.Manipulator.LastException;
            return res;
        }

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

        public bool DeleteCompanyComplaintGroups(int companyId)
        {
            string tableName = TableNameStorage.CompanyComplaintKeywordGroupsTable.Replace
                ("(n)", companyId.ToString());
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "delete from " + tableName + " where id>0;";
            return ExecuteNonQuery(cmd);
        }

        public List<CompanySettingsEntry> GetCompanySettings(int companyId)
        {
            string tableName = TableNameStorage.CompanySettingsTable.Replace("(n)", companyId.ToString());
            var setting = CompanySettingsEntry.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (setting == null)
                LastException = CompanySettingsEntry.Manipulator.LastException;
            return setting;
        }

        public CompanySettingsEntry GetCompanySettingsById(int companyId, int settingsId)
        {
            string tableName = TableNameStorage.CompanySettingsTable.Replace("(n)", companyId.ToString());
            var setting = CompanySettingsEntry.Manipulator.RetrieveDataWithId(Connection, tableName, settingsId.ToString());
            if (setting == null)
                LastException = CompanySettingsEntry.Manipulator.LastException;
            return setting;
        }

        public List<CompanySettingsEntry> GetCompanySettingsWhere(int companyId, string where)
        {
            string tableName = TableNameStorage.CompanySettingsTable.Replace("(n)", companyId.ToString());
            var settings = CompanySettingsEntry.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if(settings == null)
                LastException = CompanySettingsEntry.Manipulator.LastException;
            return settings;
        }

        public bool UpdateCompanySettings(int companyId, CompanySettingsEntry toUpdate)
        {
            string tableName = TableNameStorage.CompanySettingsTable.Replace("(n)", companyId.ToString());
            string cmdText = "update " + tableName + "set SettingKey=\"" + toUpdate.SettingKey + "\", SettingValue=\"" + toUpdate.SettingValue + "\" where id=" + toUpdate.Id+";";
            var cmd = Connection.CreateCommand();
            cmd.CommandText = cmdText;
            return ExecuteNonQuery(cmd);
        }

        public double GetCompanyAccuracy(int companyId)
        {
            var company = CompanyId.Manipulator.RetrieveDataWithId(Connection, TableNameStorage.CompanyIdTable, companyId.ToString());
            if (company == null)
                return -1;
            return company.ModelAccuracy;
        }

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
            return true;
        }

        public List<JoinRequest> GetJoinRequests(int companyId)
        {
            string tableName = TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString());
            var requests = JoinRequest.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (requests == null)
                LastException = JoinRequest.Manipulator.LastException;
            return requests;
        }

        public JoinRequest GetJoinRequestById(int companyId, int requestId)
        {
            string tableName = TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString());
            var request = JoinRequest.Manipulator.RetrieveDataWithId(Connection, tableName, requestId.ToString());
            if (request == null)
                LastException = JoinRequest.Manipulator.LastException;
            return request;
        }

        public List<JoinRequest> GetJoinRequestsWhere(int companyId, string where)
        {
            string tableName = TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString());
            var requests = JoinRequest.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if (requests == null)
                LastException = JoinRequest.Manipulator.LastException;
            return requests;
        }

        public List<JoinRequest> GetJoinRequestsByIdRange(int companyId, int idStart, int idEnd)
        {
            string tableName = TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString());
            var requests = JoinRequest.Manipulator.RetrieveDataWhere(Connection, tableName, " id >= " + idStart + " and id <" + idEnd + ";");
            if (requests == null)
                LastException = JoinRequest.Manipulator.LastException;
            return requests;
        }

        public bool AddForumPost(int companyId, int repairJobId, UserToTextEntry userForumPost)
        {
            string tableName = TableNameStorage.CompanyForumTable.Replace("(n)", companyId.ToString());
            tableName = tableName.Replace("(m)", repairJobId.ToString());
            var res = UserToTextEntry.Manipulator.InsertDataInto(Connection, tableName, userForumPost);
            if (res == -1)
                LastException = UserToTextEntry.Manipulator.LastException;
            return res == 1;
        }

        public List<UserToTextEntry> GetForumPosts(int companyId, int repairJobId)
        {
            string tableName = TableNameStorage.CompanyForumTable.Replace("(n)", companyId.ToString());
            tableName = tableName.Replace("(m)", repairJobId.ToString());
            var res = UserToTextEntry.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (res == null)
                LastException = UserToTextEntry.Manipulator.LastException;
            return res;
        }

        public UserToTextEntry GetForumPost(int companyId, int repairJobId, int forumPostId)
        {
            string tableName = TableNameStorage.CompanyForumTable.Replace("(n)", companyId.ToString());
            tableName = tableName.Replace("(m)", repairJobId.ToString());
            var res = UserToTextEntry.Manipulator.RetrieveDataWithId(Connection, tableName, forumPostId.ToString());
            if (res == null)
                LastException = UserToTextEntry.Manipulator.LastException;
            return res;
        }

        public List<UserToTextEntry> GetForumPostsWhere(int companyId, int repairJobId, string where)
        {
            string tableName = TableNameStorage.CompanyForumTable.Replace("(n)", companyId.ToString());
            tableName = tableName.Replace("(m)", repairJobId.ToString());
            var res = UserToTextEntry.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if (res == null)
                LastException = UserToTextEntry.Manipulator.LastException;
            return res;
        }

        public bool RemoveForumPost(int companyId, int repairJobId, UserToTextEntry entry)
        {
            string tableName = TableNameStorage.CompanyForumTable.Replace("(n)", companyId.ToString());
            tableName = tableName.Replace("(m)", repairJobId.ToString());
            var res = UserToTextEntry.Manipulator.RemoveDataWithId(Connection, tableName, entry.Id);
            if (res == -1)
                LastException = UserToTextEntry.Manipulator.LastException;
            return res == 1;    
        }

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

        public bool AddPartEntry(int companyId, PartCatalogueEntry toAdd)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = PartCatalogueEntry.Manipulator.InsertDataInto(Connection, tableName, toAdd);
            if (res == -1)
                LastException = PartCatalogueEntry.Manipulator.LastException;
            return res == 1;
        }

        public List<PartCatalogueEntry> GetPartCatalogueEntries(int companyId)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = PartCatalogueEntry.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (res == null)
                LastException = PartCatalogueEntry.Manipulator.LastException;
            return res;
        }

        public PartCatalogueEntry GetPartCatalogueEntryById(int companyId, int entryId)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = PartCatalogueEntry.Manipulator.RetrieveDataWithId(Connection, tableName, entryId.ToString());
            if (res == null)
                LastException = PartCatalogueEntry.Manipulator.LastException;
            return res;
        }

        public List<PartCatalogueEntry> GetPartCatalogueEntriesWhere(int companyId, string where)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = PartCatalogueEntry.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if (res == null)
                LastException = PartCatalogueEntry.Manipulator.LastException;
            return res;
        }

        public List<PartCatalogueEntry> GetPartCatalogueEntriesByIdRange(int companyId, int startId, int endId)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = PartCatalogueEntry.Manipulator.RetrieveDataWhere(Connection, tableName, " id >= " + startId + " and id <" + endId + ";");
            if (res == null)
                LastException = PartCatalogueEntry.Manipulator.LastException;
            return res;
        }

        public bool RemovePartCatalogueEntry(int companyId, int entryId)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = PartCatalogueEntry.Manipulator.RemoveDataWithId(Connection, tableName, entryId);
            if (res == -1)
                LastException = PartCatalogueEntry.Manipulator.LastException;
            return res == 1;
        }

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

        public bool RemovePartsListAdditionRequest(int companyId, int requestId, bool accept=false)
        {
            string tableName = TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString());
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
            return true;
        }

        public List<RequirementAdditionRequest> GetPartsListAdditionRequests(int companyId)
        {
            string tableName = TableNameStorage.CompanyPartsListsRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        public RequirementAdditionRequest GetPartsListAdditionRequestById(int companyId, int requirementId)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataWithId(Connection, tableName, requirementId.ToString());
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        public List<RequirementAdditionRequest> GetPartsListAdditionRequestsWhere(int companyId, string where)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        public List<RequirementAdditionRequest> GetPartsListAdditionRequestsByIdRange(int companyId, int startId, int endId)
        {
            string tableName = TableNameStorage.CompanyPartsCatalogueTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataWhere(Connection, tableName, " id >= " + startId + " and id <" + endId + ";");
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

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

        public bool RemoveSafetyAdditionRequest(int companyId, int requestId, bool accept = false)
        {
            string tableName = TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString());
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
            return true;
        }

        public List<RequirementAdditionRequest> GetSafetyAdditionRequests(int companyId)
        {
            string tableName = TableNameStorage.CompanySafetyRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        public RequirementAdditionRequest GetSafetyAdditionRequestById(int companyId, int requirementId)
        {
            string tableName = TableNameStorage.CompanySafetyRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataWithId(Connection, tableName, requirementId.ToString());
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        public List<RequirementAdditionRequest> GetSafetyAdditionRequestsWhere(int companyId, string where)
        {
            string tableName = TableNameStorage.CompanySafetyRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

        public List<RequirementAdditionRequest> GetSafetyAdditionRequestsByIdRange(int companyId, int startId, int endId)
        {
            string tableName = TableNameStorage.CompanySafetyRequestsTable.Replace("(n)", companyId.ToString());
            var res = RequirementAdditionRequest.Manipulator.RetrieveDataWhere(Connection, tableName, " id >= " + startId + " and id <" + endId + ";");
            if (res == null)
                LastException = RequirementAdditionRequest.Manipulator.LastException;
            return res;
        }

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

        public bool RemovePartsRequest(int companyId, int requestId, bool accept=false)
        {
            string tableName = TableNameStorage.CompanyJoinRequestsTable.Replace("(n)", companyId.ToString());
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

        public List<PartsRequest> GetPartsRequests(int companyId)
        {
            string tableName = TableNameStorage.CompanyPartsRequestTable.Replace("(n)", companyId.ToString());
            var res = PartsRequest.Manipulator.RetrieveDataFrom(Connection, tableName);
            if (res == null)
                LastException = PartsRequest.Manipulator.LastException;
            return res;
        }

        public PartsRequest GetPartsRequestById(int companyId, int requestId)
        {
            string tableName = TableNameStorage.CompanyPartsRequestTable.Replace("(n)", companyId.ToString());
            var res = PartsRequest.Manipulator.RetrieveDataWithId(Connection, tableName, requestId.ToString());
            if (res == null)
                LastException = PartsRequest.Manipulator.LastException;
            return res;
        }

        public List<PartsRequest> GetPartsRequestsWhere(int companyId, string where)
        {
            string tableName = TableNameStorage.CompanyPartsRequestTable.Replace("(n)", companyId.ToString());
            var res = PartsRequest.Manipulator.RetrieveDataWhere(Connection, tableName, where);
            if (res == null)
                LastException = PartsRequest.Manipulator.LastException;
            return res;
        }

        public List<PartsRequest> GetPartsRequestsByIdRange(int companyId, int startId, int endId)
        {
            string tableName = TableNameStorage.CompanyPartsRequestTable.Replace("(n)", companyId.ToString());
            var res = PartsRequest.Manipulator.RetrieveDataWhere(Connection, tableName, " id >= " + startId + " and id <" + endId + ";");
            if (res == null)
                LastException = PartsRequest.Manipulator.LastException;
            return res;
            throw new NotImplementedException();
        }

        public bool UpdateUserPreviousRequests(OverallUser toUpdate)
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "update " + TableNameStorage.OverallUserTable + " set RequestHistory=" + MysqlDataConvertingUtil.ConvertToHexString(toUpdate.RequestHistory) + " where id=" + toUpdate.UserId + ";";
            return ExecuteNonQuery(cmd);
        }

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
        public bool AddUser(string email, string password, string securityQuestion, string securityAnswer, int accessLevel = 1)
        {
            SecuritySchemaLib secLib = new SecuritySchemaLib(SHA512.Create().ComputeHash);
            OverallUser toAdd = new OverallUser
            {
                Email = email,
                SecurityQuestion = securityQuestion,
                DerivedSecurityToken = secLib.ConstructDerivedSecurityToken(Encoding.UTF8.GetBytes(email), Encoding.UTF8.GetBytes(password)),
                AccessLevel = accessLevel,
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
            return res == 1;
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
            return true;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Net;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;
using OldManInTheShopServer.Models;

namespace OldManInTheShopServer.Net.Api
{
    [DataContract]
    class ArchiveApiPostRequest
    {
        [DataMember]
        public int UserId = 0;

        [DataMember]
        public string LoginToken = null;

        [DataMember]
        public JobDataEntry Entry = null;

        [DataMember]
        public int ProblemGroupId = 0;

        [DataMember]
        public int CompanyId = 0;
    }

    [DataContract]
    class ArchiveApiPutRequest
    {
        [DataMember]
        public int UserId = 0;

        [DataMember]
        public string LoginToken = null;

        [DataMember]
        public JobDataEntry Entry = null;

        [DataMember]
        public int CompanyId = 0;
    }

    class ArchiveApi : ApiDefinition
    {
#if RELEASE
        public ArchiveApi(int port) : base("https://+:" + port + "/archive")
#elif DEBUG
        public ArchiveApi(int port) : base("http://+:" + port + "/archive")
#endif
        {
            PUT += HandlePutRequest;
        }

        private void HandlePutRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                ArchiveApiPutRequest req = JsonDataObjectUtil<ArchiveApiPutRequest>.ParseObject(ctx);
                if (!ValidatePutRequest(req))
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "Incorrect Format");
                    return;
                }
                MySqlDataManipulator connection = new MySqlDataManipulator();
                using (connection)
                {
                    bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected Server Error", "Connection to database failed");
                        return;
                    }
                    OverallUser mappedUser = connection.GetUserById(req.UserId);
                    if (mappedUser == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(mappedUser, req.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Login token was incorrect.");
                        return;
                    }
                    CompanySettingsEntry isPublicSetting = connection.GetCompanySettingsWhere(req.CompanyId, "SettingKey=\""+CompanySettingsKey.Public+"\"")[0];
                    bool isPublic = bool.Parse(isPublicSetting.SettingValue);
                    if (!isPublic && mappedUser.Company != req.CompanyId)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Cannot access other company's private data");
                        return;
                    }
                    UserSettingsEntry numPredictionsRequested = JsonDataObjectUtil<List<UserSettingsEntry>>.ParseObject(mappedUser.Settings).FirstOrDefault(entry => entry.Key.Equals(UserSettingsEntryKeys.ArchiveQueryResults));
                    if (numPredictionsRequested == null)
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "User did not contain a setting with a key " + UserSettingsEntryKeys.ArchiveQueryResults);
                        return;
                    }
                    int numRequested = int.Parse(numPredictionsRequested.Value);

                    string whereString = "";
                    bool addedWhere = false;
                    if(req.Entry.Complaint != null)
                    {
                        if (!PerformSanitization(req.Entry.Complaint))
                            return;
                        whereString += " Complaint like \"%" + req.Entry.Complaint + "%\"";
                        addedWhere = true;
                    }
                    if(req.Entry.Problem != null)
                    {
                        if (!PerformSanitization(req.Entry.Problem)) return;
                        if (addedWhere)
                            whereString += " and";
                        whereString += " Problem like \"%" + req.Entry.Problem + "%\"";
                        addedWhere = true;
                    }
                    if (req.Entry.Make != null)
                    {
                        if (!PerformSanitization(req.Entry.Make)) return;
                        if (addedWhere)
                            whereString += " and";
                        whereString += " Make like \"%" + req.Entry.Make + "%\"";
                        addedWhere = true;
                    }
                    if(req.Entry.Model != null)
                    {
                        if (!PerformSanitization(req.Entry.Model)) return;
                        if (addedWhere)
                            whereString += " and"; 
                        whereString += " Model like \"%" + req.Entry.Model + "%\"";
                        addedWhere = true;
                    }
                    if(req.Entry.Year != 0)
                    {
                        if (addedWhere)
                            whereString += " and";
                        whereString += " Year =" + req.Entry.Year;
                        addedWhere = true;
                    }
                    if (!addedWhere)
                    {
                        WriteBodyResponse(ctx, 400, "Bad Request", "No fields in the request's entry were filled");
                        return;
                    }
                    List<JobDataEntry> entries = connection.GetDataEntriesWhere(req.CompanyId, whereString, true);
                    JsonListStringConstructor retConstructor = new JsonListStringConstructor();
                    try
                    {
                        entries.ForEach(entry => retConstructor.AddElement(ConvertEntry(entry)));
                    } catch(NullReferenceException)
                    {
                        WriteBodyResponse(ctx, 200, "OK", "[]", "application/json");
                        return;
                    }
                    WriteBodyResponse(ctx, 200, "OK", retConstructor.ToString(), "application/json");

                    bool PerformSanitization(string queryIn)
                    {
                        if(queryIn.Contains('`'))
                        {
                            WriteBodyResponse(ctx, 400, "Bad Request", "Request contained the single quote character, which is disallowed due to MySQL injection attacks");
                            return false;
                        }
                        return true;
                    }
                }
            }
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occurred during processing of request: " + e.Message);
            }
        }

        private JsonDictionaryStringConstructor ConvertEntry(JobDataEntry e)
        {
            JsonDictionaryStringConstructor r = new JsonDictionaryStringConstructor();
            r.SetMapping("Make", e.Make);
            r.SetMapping("Model", e.Model);
            r.SetMapping("Complaint", e.Complaint);
            r.SetMapping("Problem", e.Problem);
            if (e.Year == -1)
                r.SetMapping("Year", "Unknown");
            else
                r.SetMapping("Year", e.Year);
            r.SetMapping("Id", e.Id);
            return r;
        }

        private bool ValidatePutRequest(ArchiveApiPutRequest req)
        {
            if (req.Entry == null)
                return false;
            if (req.LoginToken == null)
                return false;
            if (req.CompanyId <= 0)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }

    }
}

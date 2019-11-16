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
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public JobDataEntry Entry;

        [DataMember]
        public int ProblemGroupId;

        [DataMember]
        public int CompanyId;
    }

    [DataContract]
    class ArchiveApiPutRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public JobDataEntry Entry;

        [DataMember]
        public int CompanyId;
    }

    class ArchiveApi : ApiDefinition
    {
#if RELEASE
        public ArchiveApi(int port) : base("https://+:" + port + "/archive")
#elif DEBUG
        public ArchiveApi(int port) : base("http://+:" + port + "/archive")
#endif
        {
            POST += HandlePostRequest;
            PUT += HandlePutRequest;
        }

        private void HandlePostRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                ArchiveApiPostRequest req = JsonDataObjectUtil<ArchiveApiPostRequest>.ParseObject(ctx);
                if (!ValidateGetRequest(req))
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
                    CompanySettingsEntry isPublicSetting = connection.GetCompanySettingsWhere(req.CompanyId, "SettingKey=\"Is Public\"")[0];
                    bool isPublic = bool.Parse(isPublicSetting.SettingValue);
                    if (!isPublic && mappedUser.Company != req.CompanyId)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Cannot predict using other company's private data");
                        return;
                    }
                    List<UserSettingsEntry> userSettings = JsonDataObjectUtil<List<UserSettingsEntry>>.ParseObject(mappedUser.Settings);
                    UserSettingsEntry predictionQueryResultsSetting = userSettings.Where(entry => entry.Key.Equals(UserSettingsEntryKeys.ArchiveQueryResults)).First();
                    int numQueriesRequested = int.Parse(predictionQueryResultsSetting.Value);
                    DatabaseQueryProcessor processor = new DatabaseQueryProcessor();
                    string ret = processor.ProcessQueryForSimilarQueriesArchive(req.Entry, connection, req.CompanyId, req.ProblemGroupId, numQueriesRequested);
                    WriteBodyResponse(ctx, 200, "OK", ret, "application/json");
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
                    CompanySettingsEntry isPublicSetting = connection.GetCompanySettingsWhere(req.CompanyId, "SettingKey=\"Is Public\"")[0];
                    bool isPublic = bool.Parse(isPublicSetting.SettingValue);
                    if (!isPublic && mappedUser.Company != req.CompanyId)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Cannot predict using other company's private data");
                        return;
                    }
                    UserSettingsEntry numPredictionsRequested = JsonDataObjectUtil<List<UserSettingsEntry>>.ParseObject(mappedUser.Settings).FirstOrDefault(entry => entry.Key.Equals(UserSettingsEntryKeys.ProblemGroupResults));
                    if (numPredictionsRequested == null)
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "User did not contain a setting with a key " + UserSettingsEntryKeys.ProblemGroupResults);
                        return;
                    }
                    int numRequested = int.Parse(numPredictionsRequested.Value);
                    DatabaseQueryProcessor processor = new DatabaseQueryProcessor();
                    string ret = processor.ProcessQueryForProblemGroups(req.Entry, connection, req.CompanyId, numRequested);
                    WriteBodyResponse(ctx, 200, "OK", ret, "application/json");
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

        private bool ValidateGetRequest(ArchiveApiPostRequest req)
        {
            if (req.Entry == null)
                return false;
            if (req.LoginToken == null)
                return false;
            if (req.CompanyId <= 0)
                return false;
            if (req.ProblemGroupId < 0)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
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

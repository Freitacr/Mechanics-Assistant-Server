using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Net;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Net.Api
{
    [DataContract]
    class CompanySafetyRequestApiFullPostRequest
    {
        [DataMember]
        public int UserId;
        [DataMember]
        public string LoginToken;
        [DataMember]
        public string AuthToken;
        [DataMember]
        public int RepairJobId;
        [DataMember]
        public string SafetyRequirements;
        [DataMember]
        public int CompanyId;
    }

    [DataContract]
    class CompanySafetyRequestApiGetRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;

    }

    [DataContract]
    class CompanySafetyRequestApiPutRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;

        [DataMember]
        public int RequestId;
    }

    [DataContract]
    class CompanySafetyRequestApiDeleteRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;

        [DataMember]
        public int RequestId;
    }

    [DataContract]
    class CompanySafetyRequestApiPatchRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;

        [DataMember]
        public int RequestId;

        [DataMember]
        public int RequirementId;

        [DataMember]
        public string SafetyRequirementsString;
    }

    class CompanySafetyRequestApi : ApiDefinition
    {
#if RELEASE
        public CompanySafetyRequestApi(int port) : base("https://+:" + port + "/company/safety/request")
#elif DEBUG
        public CompanySafetyRequestApi(int port) : base("http://+:" + port + "/company/safety/request")
#endif
        {
            POST += HandlePostRequest;
            GET += HandleGetRequest;
            DELETE += HandleDeleteRequest;
            PATCH += HandlePatchRequest;
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
                CompanySafetyRequestApiFullPostRequest entry = JsonDataObjectUtil<CompanySafetyRequestApiFullPostRequest>.ParseObject(ctx);
                if (!ValidateFullPostRequest(entry))
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
                    OverallUser mappedUser = connection.GetUserById(entry.UserId);
                    if (mappedUser == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(mappedUser, entry.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Login token was incorrect.");
                        return;
                    }
                    if (!UserVerificationUtil.AuthTokenValid(mappedUser, entry.AuthToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Auth token was ezpired or incorrect");
                        return;
                    }

                    //build safety request
                    RequirementAdditionRequest request = new RequirementAdditionRequest(entry.UserId, entry.RepairJobId, entry.SafetyRequirements);
                    //send request
                    res = connection.AddSafetyAdditionRequest(entry.CompanyId,request);
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected Serer Error", connection.LastException.Message);
                        return;
                    }
                    WriteBodylessResponse(ctx,200,"OK");
                }
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private bool ValidateFullPostRequest(CompanySafetyRequestApiFullPostRequest req)
        {
            if (req.UserId <= 0)
                return false;
            if (req.CompanyId <= 0)
                return false;
            if (req.AuthToken == null || req.AuthToken.Equals(""))
                return false;
            if (req.LoginToken == null || req.LoginToken.Equals(""))
                return false;
            if (req.RepairJobId <= 0)
                return false;
            if (req.SafetyRequirements == null || req.SafetyRequirements.Equals(""))
                return false;
            return true;
        }
        private void HandleGetRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                CompanySafetyRequestApiGetRequest entry = JsonDataObjectUtil<CompanySafetyRequestApiGetRequest>.ParseObject(ctx);
                if (!ValidateGetRequest(entry))
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
                    OverallUser mappedUser = connection.GetUserById(entry.UserId);
                    if (mappedUser == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(mappedUser, entry.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Login token was incorrect.");
                        return;
                    }
                    if (!UserVerificationUtil.AuthTokenValid(mappedUser, entry.AuthToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Auth token was ezpired or incorrect");
                        return;
                    }
                    if ((mappedUser.AccessLevel & AccessLevelMasks.SafetyMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not a parts level user");
                        return;
                    }

                    List<RequirementAdditionRequest> requests = connection.GetSafetyAdditionRequests(mappedUser.Company);
                    JsonListStringConstructor retConstructor = new JsonListStringConstructor();
                    requests.ForEach(req => retConstructor.AddElement(WriteSafetyRequestToOutput(req, connection, mappedUser.Company)));

                    WriteBodyResponse(ctx, 200, "OK", retConstructor.ToString());
                }
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }


        private JsonDictionaryStringConstructor WriteSafetyRequestToOutput(RequirementAdditionRequest request, MySqlDataManipulator connection, int companyId)
        {
            JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
            var user = connection.GetUserById(request.UserId);
            if (user == null)
            {
                ret.SetMapping("DisplayName", "Unknown User");
            }
            else
            {
                List<SettingsEntry> userSettings = JsonDataObjectUtil<List<SettingsEntry>>.ParseObject(user.Settings);
                ret.SetMapping("DisplayName", userSettings.Where(entry => entry.Key.Equals("displayName")).First().Value);
            }

            List<string> requestedPartAdditions = JsonDataObjectUtil<List<string>>.ParseObject(request.RequestedAdditions);
            JsonListStringConstructor partsConstructor = new JsonListStringConstructor();
            foreach (string i in requestedPartAdditions)
            {
                partsConstructor.AddElement(i);
            }
            ret.SetMapping("RequestedAdditions", partsConstructor);
            var job = connection.GetDataEntryById(companyId, request.ValidatedDataId);
            if (job == null)
            {
                ret.SetMapping("JobId", "Unknown");
            }
            else
            {
                ret.SetMapping("JobId", job.JobId);
            }
            ret.SetMapping("Id", request.Id);
            return ret;
        }

        private void HandleDeleteRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                CompanySafetyRequestApiDeleteRequest entry = JsonDataObjectUtil<CompanySafetyRequestApiDeleteRequest>.ParseObject(ctx);
                if (!ValidateDeleteRequest(entry))
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
                    OverallUser mappedUser = connection.GetUserById(entry.UserId);
                    if (mappedUser == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(mappedUser, entry.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Login token was incorrect.");
                        return;
                    }
                    if (!UserVerificationUtil.AuthTokenValid(mappedUser, entry.AuthToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Auth token was ezpired or incorrect");
                        return;
                    }
                    if ((mappedUser.AccessLevel & AccessLevelMasks.SafetyMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not a parts level user");
                        return;
                    }

                    var request = connection.GetSafetyAdditionRequestById(mappedUser.Company, entry.RequestId);

                    if (request == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "The requested request was not found");
                        return;
                    }
                    if (!connection.RemoveSafetyAdditionRequest(mappedUser.Company, entry.RequestId, accept: false))
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error ocurred while removing request: " + connection.LastException.Message);
                        return;
                    }

                    WriteBodylessResponse(ctx, 200, "OK");
                }
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private void HandlePatchRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                CompanySafetyRequestApiPatchRequest entry = JsonDataObjectUtil<CompanySafetyRequestApiPatchRequest>.ParseObject(ctx);
                if (!ValidatePatchRequest(entry))
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
                    OverallUser mappedUser = connection.GetUserById(entry.UserId);
                    if (mappedUser == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(mappedUser, entry.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Login token was incorrect.");
                        return;
                    }
                    if (!UserVerificationUtil.AuthTokenValid(mappedUser, entry.AuthToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Auth token was ezpired or incorrect");
                        return;
                    }
                    if ((mappedUser.AccessLevel & AccessLevelMasks.SafetyMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not a parts level user");
                        return;
                    }

                    var request = connection.GetSafetyAdditionRequestById(mappedUser.Company, entry.RequestId);

                    if (request == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "The requested request was not found");
                        return;
                    }

                    List<string> requestedParts = JsonDataObjectUtil<List<string>>.ParseObject(request.RequestedAdditions);
                    if (entry.RequirementId >= requestedParts.Count)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "The requested part to change was not found");
                        return;
                    }
                    requestedParts[entry.RequirementId] = entry.SafetyRequirementsString;
                    JsonListStringConstructor editConstructor = new JsonListStringConstructor();
                    foreach (string i in requestedParts)
                        editConstructor.AddElement(i);
                    request.RequestedAdditions = editConstructor.ToString();
                    if (!connection.UpdateSafetyAdditionRequest(mappedUser.Company, request))
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occurred while attempting to update request: " + connection.LastException.Message);
                        return;
                    }
                    WriteBodylessResponse(ctx, 200, "OK");
                }
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
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
                CompanySafetyRequestApiPutRequest entry = JsonDataObjectUtil<CompanySafetyRequestApiPutRequest>.ParseObject(ctx);
                if (!ValidatePutRequest(entry))
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
                    OverallUser mappedUser = connection.GetUserById(entry.UserId);
                    if (mappedUser == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(mappedUser, entry.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Login token was incorrect.");
                        return;
                    }
                    if (!UserVerificationUtil.AuthTokenValid(mappedUser, entry.AuthToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Auth token was ezpired or incorrect");
                        return;
                    }
                    if ((mappedUser.AccessLevel & AccessLevelMasks.SafetyMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not a parts level user");
                        return;
                    }

                    var request = connection.GetSafetyAdditionRequestById(mappedUser.Company, entry.RequestId);

                    if (request == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "The requested request was not found");
                        return;
                    }
                    if (!connection.RemoveSafetyAdditionRequest(mappedUser.Company, entry.RequestId, accept: true))
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error ocurred while removing request: " + connection.LastException.Message);
                        return;
                    }

                    WriteBodylessResponse(ctx, 200, "OK");
                }
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private bool ValidateGetRequest(CompanySafetyRequestApiGetRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.AuthToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }

        private bool ValidatePutRequest(CompanySafetyRequestApiPutRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.AuthToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            if (req.RequestId <= 0)
                return false;
            return true;
        }

        private bool ValidateDeleteRequest(CompanySafetyRequestApiDeleteRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.AuthToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            if (req.RequestId <= 0)
                return false;
            return true;
        }

        private bool ValidatePatchRequest(CompanySafetyRequestApiPatchRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.AuthToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            if (req.RequestId <= 0)
                return false;
            if (req.RequirementId <= -1)
                return false;
            if (req.SafetyRequirementsString == null)
                return false;
            return true;
        }
    }
}

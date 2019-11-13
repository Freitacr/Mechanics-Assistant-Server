using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Net;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;
using System.IO;

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
            PATCH += HandlePatchRequest;
            PUT += HandlePutRequest;
        }

        /// <summary>
        /// POST request format located in the Web Api Enumeration v2
        /// under the tab Company/Safety/Request, starting row 1
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
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
                    List<string> safetyRequirements = JsonDataObjectUtil<List<string>>.ParseObject(entry.SafetyRequirements);
                    //build safety request
                    foreach (string requirement in safetyRequirements)
                    {
                        RequirementAdditionRequest request = new RequirementAdditionRequest(entry.UserId, entry.RepairJobId, requirement);
                        //send request
                        res = connection.AddSafetyAdditionRequest(entry.CompanyId, request);
                        if (!res)
                        {
                            WriteBodyResponse(ctx, 500, "Unexpected Serer Error", connection.LastException.Message);
                            return;
                        }
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

        /// <summary>
        /// GET request format located in the Web Api Enumeration v2
        /// under the tab Company/Safety/Request, starting row 23
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandleGetRequest(HttpListenerContext ctx, CompanySafetyRequestApiGetRequest entry)
        {
            try
            {
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
            ret.SetMapping("RequestedAdditions", request.RequestedAdditions);
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

        /// <summary>
        /// DELETE request format located in the Web Api Enumeration v2
        /// under the tab Company/Safety/Request, starting row 73
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandleDeleteRequest(HttpListenerContext ctx, CompanySafetyRequestApiDeleteRequest entry)
        {
            try
            {
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

        /// <summary>
        /// PATCH request format located in the Web Api Enumeration v2
        /// under the tab Company/Safety/Request, starting row 95
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandlePatchRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                string reqStr = new StreamReader(ctx.Request.InputStream).ReadToEnd();
                CompanySafetyRequestApiPatchRequest entry = JsonDataObjectUtil<CompanySafetyRequestApiPatchRequest>.ParseObject(reqStr);
                if (!ValidatePatchRequest(entry))
                {
                    CompanySafetyRequestApiDeleteRequest entry2 = JsonDataObjectUtil<CompanySafetyRequestApiDeleteRequest>.ParseObject(reqStr);
                    if(entry2 != null && ValidateDeleteRequest(entry2))
                    {
                        HandleDeleteRequest(ctx,entry2);
                        return;
                    }
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

                    request.RequestedAdditions = entry.SafetyRequirementsString;
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

        /// <summary>
        /// PUT request format located in the Web Api Enumeration v2
        /// under the tab Company/Safety/Request, starting row 49
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandlePutRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                string reqStr = new StreamReader(ctx.Request.InputStream).ReadToEnd();
                CompanySafetyRequestApiPutRequest entry = JsonDataObjectUtil<CompanySafetyRequestApiPutRequest>.ParseObject(reqStr);
                if (!ValidatePutRequest(entry))
                {
                    CompanySafetyRequestApiGetRequest entry2 = JsonDataObjectUtil<CompanySafetyRequestApiGetRequest>.ParseObject(reqStr);
                    if (entry2 != null && ValidateGetRequest(entry2))
                    {
                        HandleGetRequest(ctx, entry2);
                        return;
                    }
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
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not a safety level user");
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
            if (req.SafetyRequirementsString == null)
                return false;
            return true;
        }
    }
}

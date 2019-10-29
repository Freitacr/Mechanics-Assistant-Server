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
    class CompanyPartslistsRequestApiFullPostRequest
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
        public string RequiredPartsList;
        [DataMember]
        public int CompanyId;
    }

    [DataContract]
    class CompanyPartsListsRequestApiGetRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;

    }

    [DataContract]
    class CompanyPartsListsRequestApiPutRequest
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
    class CompanyPartsListsRequestApiDeleteRequest
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
    class CompanyPartsListsRequestApiPatchRequest
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
        public int PartsRequirement;
    }



    class CompanyPartslistsRequestsApi : ApiDefinition
    {
#if RELEASE
        public CompanyPartslistsRequestsApi(int port) : base("https://+:" + port + "/company/partslists/request")
#elif DEBUG
        public CompanyPartslistsRequestsApi(int port) : base("http://+:" + port + "/company/partslists/request")
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
                CompanyPartslistsRequestApiFullPostRequest entry = JsonDataObjectUtil<CompanyPartslistsRequestApiFullPostRequest>.ParseObject(ctx);
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

                    RequirementAdditionRequest request = new RequirementAdditionRequest(entry.UserId,entry.RepairJobId,entry.RequiredPartsList);
                    res = connection.AddPartsListAdditionRequest(entry.CompanyId,request);
                    if (!res)
                    {
                        WriteBodyResponse(ctx,500,"Unexpected Server Error",connection.LastException.Message);
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

        private bool ValidateFullPostRequest(CompanyPartslistsRequestApiFullPostRequest req)
        {
            if (req.UserId == -1)
                return false;
            if (req.CompanyId == -1)
                return false;
            if (req.RepairJobId == -1)
                return false;
            if (req.LoginToken == null || req.LoginToken.Equals(""))
                return false;
            if (req.AuthToken == null || req.AuthToken.Equals(""))
                return false;
            if (req.RequiredPartsList == null || req.RequiredPartsList.Equals(""))
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
                CompanyPartsListsRequestApiGetRequest entry = JsonDataObjectUtil<CompanyPartsListsRequestApiGetRequest>.ParseObject(ctx);
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
                    if((mappedUser.AccessLevel & AccessLevelMasks.PartMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not a parts level user");
                        return;
                    }

                    List<RequirementAdditionRequest> requests = connection.GetPartsListAdditionRequests(mappedUser.Company);
                    JsonListStringConstructor retConstructor = new JsonListStringConstructor();
                    requests.ForEach(req => retConstructor.AddElement(WritePartsListRequestToOutput(req, connection, mappedUser.Company)));
                    
                    WriteBodyResponse(ctx, 200, "OK", retConstructor.ToString());
                }
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private JsonDictionaryStringConstructor WritePartsListRequestToOutput(RequirementAdditionRequest request, MySqlDataManipulator connection, int companyId)
        {
            JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
            var user = connection.GetUserById(request.UserId);
            if(user == null)
            {
                ret.SetMapping("DisplayName", "Unknown User");
            } else
            {
                List<SettingsEntry> userSettings = JsonDataObjectUtil<List<SettingsEntry>>.ParseObject(user.Settings);
                ret.SetMapping("DisplayName", userSettings.Where(entry => entry.Key.Equals("displayName")).First().Value);
            }

            List<int> requestedPartAdditions = JsonDataObjectUtil<List<int>>.ParseObject(request.RequestedAdditions);
            JsonListStringConstructor partsConstructor = new JsonListStringConstructor();
            foreach (int i in requestedPartAdditions)
            {
                var part = connection.GetPartCatalogueEntryById(companyId, i);
                if (part == null)
                    continue;
                partsConstructor.AddElement(part.PartId);
            }
            ret.SetMapping("RequestedAdditions", partsConstructor);
            var job = connection.GetDataEntryById(companyId, request.ValidatedDataId);
            if(job == null)
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

        private void HandlePutRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                CompanyPartsListsRequestApiPutRequest entry = JsonDataObjectUtil<CompanyPartsListsRequestApiPutRequest>.ParseObject(ctx);
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
                    if ((mappedUser.AccessLevel & AccessLevelMasks.PartMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not a parts level user");
                        return;
                    }

                    var request = connection.GetPartsListAdditionRequestById(mappedUser.Company, entry.RequestId);

                    if(request == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "The requested request was not found");
                        return;
                    }
                    if(!connection.RemovePartsListAdditionRequest(mappedUser.Company, entry.RequestId, accept: true))
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

        private void HandleDeleteRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                CompanyPartsListsRequestApiDeleteRequest entry = JsonDataObjectUtil<CompanyPartsListsRequestApiDeleteRequest>.ParseObject(ctx);
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
                    if ((mappedUser.AccessLevel & AccessLevelMasks.PartMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not a parts level user");
                        return;
                    }

                    var request = connection.GetPartsListAdditionRequestById(mappedUser.Company, entry.RequestId);

                    if (request == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "The requested request was not found");
                        return;
                    }
                    if (!connection.RemovePartsListAdditionRequest(mappedUser.Company, entry.RequestId, accept: false))
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
                CompanyPartsListsRequestApiPatchRequest entry = JsonDataObjectUtil<CompanyPartsListsRequestApiPatchRequest>.ParseObject(ctx);
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
                    if ((mappedUser.AccessLevel & AccessLevelMasks.PartMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not a parts level user");
                        return;
                    }

                    var request = connection.GetPartsListAdditionRequestById(mappedUser.Company, entry.RequestId);

                    if (request == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "The requested request was not found");
                        return;
                    }

                    List<int> requestedParts = JsonDataObjectUtil<List<int>>.ParseObject(request.RequestedAdditions);
                    if (entry.RequirementId >= requestedParts.Count)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "The requested part to change was not found");
                        return;
                    }
                    requestedParts[entry.RequirementId] = entry.PartsRequirement;
                    JsonListStringConstructor editConstructor = new JsonListStringConstructor();
                    foreach (int i in requestedParts)
                        editConstructor.AddElement(i);
                    request.RequestedAdditions = editConstructor.ToString();
                    if(!connection.UpdatePartsListAdditionRequest(mappedUser.Company, request))
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
    
        private bool ValidateGetRequest(CompanyPartsListsRequestApiGetRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.AuthToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }

        private bool ValidatePutRequest(CompanyPartsListsRequestApiPutRequest req)
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

        private bool ValidateDeleteRequest(CompanyPartsListsRequestApiDeleteRequest req)
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

        private bool ValidatePatchRequest(CompanyPartsListsRequestApiPatchRequest req)
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
            if (req.PartsRequirement <= 0)
                return false;
            return true;
        }




    }
}

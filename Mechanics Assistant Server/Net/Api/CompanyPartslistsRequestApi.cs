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
    class CompanyPartslistsRequestApiFullPostRequest
    {
        [DataMember]
        public int UserId = default;
        [DataMember]
        public string LoginToken = default;
        [DataMember]
        public string AuthToken = default;
        [DataMember]
        public int RepairJobId = default;
        [DataMember]
        public string RequiredPartsList = default;
        [DataMember]
        public int CompanyId = default;
    }

    [DataContract]
    class CompanyPartsListsRequestApiGetRequest
    {
        [DataMember]
        public int UserId = default;

        [DataMember]
        public string LoginToken = default;

        [DataMember]
        public string AuthToken = default;

    }

    [DataContract]
    class CompanyPartsListsRequestApiPutRequest
    {
        [DataMember]
        public int UserId = default;

        [DataMember]
        public string LoginToken = default;

        [DataMember]
        public string AuthToken = default;

        [DataMember]
        public int RequestId = default;
    }

    [DataContract]
    class CompanyPartsListsRequestApiDeleteRequest
    {
        [DataMember]
        public int UserId = default;

        [DataMember]
        public string LoginToken = default;

        [DataMember]
        public string AuthToken = default;

        [DataMember]
        public int RequestId = default;
    }

    [DataContract]
    class CompanyPartsListsRequestApiPatchRequest
    {
        [DataMember]
        public int UserId = default;

        [DataMember]
        public string LoginToken = default;

        [DataMember]
        public string AuthToken = default;

        [DataMember]
        public int RequestId = default;

        [DataMember]
        public int RequirementId = default;

        [DataMember]
        public int PartsRequirement = default;
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
            PATCH += HandlePatchRequest;
            PUT += HandlePutRequest;
        }

        /// <summary>
        /// POST request format located in the Web Api Enumeration v2
        /// under the tab Company/Partslists/Request, starting row 1
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
                    List<int> requiredPartsList = JsonDataObjectUtil<List<int>>.ParseObject(entry.RequiredPartsList);
                    foreach(int i in requiredPartsList)
                    {
                        RequirementAdditionRequest request = new RequirementAdditionRequest(entry.UserId,entry.RepairJobId, i.ToString());
                    
                        res = connection.AddPartsListAdditionRequest(entry.CompanyId,request);
                        if (!res)
                        {
                            WriteBodyResponse(ctx,500,"Unexpected Server Error",connection.LastException.Message);
                            return;
                        }

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

        /// <summary>
        /// GET request format located in the Web Api Enumeration v2
        /// under the tab Company/Partslists/Request, starting row 23
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandleGetRequest(HttpListenerContext ctx, CompanyPartsListsRequestApiGetRequest entry)
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
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
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
                List<UserSettingsEntry> userSettings = JsonDataObjectUtil<List<UserSettingsEntry>>.ParseObject(user.Settings);
                ret.SetMapping("DisplayName", userSettings.Where(entry => entry.Key.Equals(UserSettingsEntryKeys.DisplayName)).First().Value);
            }

            var part = connection.GetPartCatalogueEntryById(companyId, int.Parse(request.RequestedAdditions));
            if(part == null)
            {
                ret.SetMapping("RequestedAdditions", "Unknown Part");
            } else
            {
                ret.SetMapping("RequestedAdditions", part.PartId);
            }
            JsonListStringConstructor partsConstructor = new JsonListStringConstructor();
            
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

        /// <summary>
        /// PULL request format located in the Web Api Enumeration v2
        /// under the tab Company/Partslists/Request, starting row 49
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
                string reqStr;
                using (var reader = new StreamReader(ctx.Request.InputStream))
                {
                    reqStr = reader.ReadToEnd();
                }
                CompanyPartsListsRequestApiPutRequest entry = JsonDataObjectUtil<CompanyPartsListsRequestApiPutRequest>.ParseObject(reqStr);
                if (!ValidatePutRequest(entry))
                {
                    var entry2 = JsonDataObjectUtil<CompanyPartsListsRequestApiGetRequest>.ParseObject(reqStr);
                    if(entry2 != null && ValidateGetRequest(entry2))
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
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        /// <summary>
        /// DELETE request format located in the Web Api Enumeration v2
        /// under the tab Company/Partslists/Request, starting row 73
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandleDeleteRequest(HttpListenerContext ctx, CompanyPartsListsRequestApiDeleteRequest entry)
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
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        /// <summary>
        /// PATCH request format located in the Web Api Enumeration v2
        /// under the tab Company/Partslists/Request, starting row 95
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
                string reqStr;
                using (var reader = new StreamReader(ctx.Request.InputStream))
                {
                    reqStr = reader.ReadToEnd();
                }
                CompanyPartsListsRequestApiPatchRequest entry = JsonDataObjectUtil<CompanyPartsListsRequestApiPatchRequest>.ParseObject(reqStr);
                if (!ValidatePatchRequest(entry))
                {
                    CompanyPartsListsRequestApiDeleteRequest entry2 = JsonDataObjectUtil<CompanyPartsListsRequestApiDeleteRequest>.ParseObject(reqStr);
                    if(entry2 != null && ValidateDeleteRequest(entry2))
                    {
                        HandleDeleteRequest(ctx, entry2);
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
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
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

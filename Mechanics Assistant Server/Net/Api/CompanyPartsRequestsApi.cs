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
    class CompanyPartsRequestApiFullPostRequest
    {
        [DataMember]
        public int UserId = default;
        [DataMember]
        public string LoginToken = default;
        [DataMember]
        public string AuthToken = default;
        [DataMember]
        public string RepairJobId = default;
        [DataMember]
        public int CompanyId = default;
        [DataMember]
        public string PartsList = default;
    }

    [DataContract]
    class CompanyPartsRequestGetRequest
    {
        [DataMember]
        public int UserId = default;

        [DataMember]
        public string LoginToken = default;

        [DataMember]
        public string AuthToken = default;
    }

    [DataContract]
    class CompanyPartsRequestPutRequest
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
    class CompanyPartsRequestDeleteRequest
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



    class CompanyPartsRequestApi : ApiDefinition
    {
#if RELEASE
        public CompanyPartsRequestApi(int port) : base("https://+:" + port + "/company/parts/request")
#elif DEBUG
        public CompanyPartsRequestApi(int port) : base("http://+:" + port + "/company/parts/request")
#endif
        {
            POST += HandlePostRequest;
            PATCH += HandleDeleteRequest; //Delete reqeusts cannot contain bodies in Javascript, this is therefore intentional to get around that
            PUT += HandlePutRequest;
        }

        /// <summary>
        /// POST request format located in the Web Api Enumeration v2
        /// under the tab Company/Parts/Request, starting row 1
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
                CompanyPartsRequestApiFullPostRequest entry = JsonDataObjectUtil<CompanyPartsRequestApiFullPostRequest>.ParseObject(ctx);
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

                    //build request to send
                    PartsRequest request = new PartsRequest(entry.UserId, entry.RepairJobId, entry.PartsList);
                    //send request
                    res = connection.AddPartsRequest(entry.CompanyId, request);
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
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

        private bool ValidateFullPostRequest(CompanyPartsRequestApiFullPostRequest req)
        {
            if (req.LoginToken == null || req.LoginToken.Equals(""))
                return false;
            if (req.AuthToken == null || req.AuthToken.Equals(""))
                return false;
            if (req.CompanyId == -1)
                return false;
            if (req.UserId == -1)
                return false;
            if (req.PartsList == null || req.PartsList.Equals(""))
                return false;
            if (req.RepairJobId == null || req.RepairJobId.Equals(""))
                return false;
            return true;
        }

        /// <summary>
        /// GET request format located in the Web Api Enumeration v2
        /// under the tab Company/Parts/Request, starting row 23
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandleGetRequest(HttpListenerContext ctx, CompanyPartsRequestGetRequest entry)
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
                    JsonListStringConstructor retConstructor = new JsonListStringConstructor();
                    List<PartsRequest> requests = connection.GetPartsRequests(mappedUser.Company);
                    requests.ForEach(req => retConstructor.AddElement(WritePartsRequestToOutput(req, connection, mappedUser.Company)));

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

        private JsonDictionaryStringConstructor WritePartsRequestToOutput(PartsRequest request, MySqlDataManipulator manipulator, int companyId)
        {
            JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
            var user = manipulator.GetUserById(request.UserId);
            if (user == null)
            {
                ret.SetMapping("DisplayName", "defaultUser");
            }
            else
            {
                List<UserSettingsEntry> entries = JsonDataObjectUtil<List<UserSettingsEntry>>.ParseObject(user.Settings);
                ret.SetMapping("DisplayName", entries.Where(entry => entry.Key.Equals(UserSettingsEntryKeys.DisplayName)).First().Value);
            }
            List<int> referencedParts = JsonDataObjectUtil<List<int>>.ParseObject(request.ReferencedParts);
            JsonListStringConstructor partsConstructor = new JsonListStringConstructor();
            foreach (int i in referencedParts)
            {
                var part = manipulator.GetPartCatalogueEntryById(companyId, i);
                if (part == null)
                    continue;
                partsConstructor.AddElement(part.PartId);
            }
            ret.SetMapping("ReferencedParts", partsConstructor);
            ret.SetMapping("JobId", request.JobId);
            ret.SetMapping("Id", request.Id);
            return ret;
        }

        /// <summary>
        /// DELETE request format located in the Web Api Enumeration v2
        /// under the tab Company/Parts/Request, starting row 71
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandleDeleteRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                CompanyPartsRequestDeleteRequest entry = JsonDataObjectUtil<CompanyPartsRequestDeleteRequest>.ParseObject(ctx);
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
                    PartsRequest request = connection.GetPartsRequestById(mappedUser.Company, entry.RequestId);
                    if (request == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "Parts Request with the given id was not found on the server");
                        return;
                    }
                    if (!connection.RemovePartsRequest(mappedUser.Company, entry.RequestId, accept: false))
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error ocurred while removing the parts request: " + connection.LastException.Message);
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
        /// PUT request format located in the Web Api Enumeration v2
        /// under the tab Company/Parts/Request, starting row 49
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
                CompanyPartsRequestPutRequest entry = JsonDataObjectUtil<CompanyPartsRequestPutRequest>.ParseObject(reqStr);
                if (!ValidatePutRequest(entry))
                {
                    CompanyPartsRequestGetRequest entry2 = JsonDataObjectUtil<CompanyPartsRequestGetRequest>.ParseObject(reqStr);
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
                    PartsRequest request = connection.GetPartsRequestById(mappedUser.Company, entry.RequestId);
                    if (request == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "Parts Request with the given id was not found on the server");
                        return;
                    }
                    if (!connection.RemovePartsRequest(mappedUser.Company, entry.RequestId, accept: true))
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error ocurred while removing the parts request: " + connection.LastException.Message);
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


        private bool ValidateGetRequest(CompanyPartsRequestGetRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.AuthToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }


        private bool ValidateDeleteRequest(CompanyPartsRequestDeleteRequest req)
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

        private bool ValidatePutRequest(CompanyPartsRequestPutRequest req)
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
    }
}

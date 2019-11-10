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
    class CompanyRequestsApiFullPostRequest
    {
        [DataMember]
        public int UserId;
        [DataMember]
        public string LoginToken;
        [DataMember]
        public string AuthToken;
        [DataMember]
        public int CompanyId;
    }

    [DataContract]
    class CompanyRequestsGetRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;
    }

    [DataContract]
    class CompanyRequestsPutRequest
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
    class CompanyRequestsPatchRequest
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

    class CompanyRequestsApi : ApiDefinition
    {
#if RELEASE
        public CompanyRequestsApi(int port) : base("https://+:" + port + "/company/requests")
#elif DEBUG
        public CompanyRequestsApi(int port) : base("http://+:" + port + "/company/requests")
#endif
        {
            POST += HandlePostRequest;
            PATCH += HandlePatchRequest;
            PUT += HandlePutRequest;
        }

        /// <summary>
        /// POST request format located in the Web Api Enumeration v2
        /// under the tab Company/Requests, starting row 1
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
                CompanyRequestsApiFullPostRequest entry = JsonDataObjectUtil<CompanyRequestsApiFullPostRequest>.ParseObject(ctx);
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

                    res = connection.AddJoinRequest(entry.CompanyId,entry.UserId);
                    if (!res)
                    {
                        WriteBodyResponse(ctx,500,"Unexpected Server Error", connection.LastException.Message);
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

        private bool ValidateFullPostRequest(CompanyRequestsApiFullPostRequest req)
        {
            if (req.UserId == -1)
                return false;
            if (req.CompanyId == -1)
                return false;
            if (req.LoginToken == null || req.LoginToken.Equals(""))
                return false;
            if (req.AuthToken == null || req.AuthToken.Equals(""))
                return false;
            return true;
        }

        /// <summary>
        /// GET request format located in the Web Api Enumeration v2
        /// under the tab Company/Requests, starting row 23
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandleGetRequest(HttpListenerContext ctx, CompanyRequestsGetRequest entry)
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
                    if ((mappedUser.AccessLevel & AccessLevelMasks.AdminMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not an administrative user");
                        return;
                    }
                    var requests = connection.GetJoinRequests(mappedUser.Company);
                    JsonListStringConstructor returnConstructor = new JsonListStringConstructor();
                    requests.ForEach(req => returnConstructor.AddElement(WriteJoinRequestToOutput(req, connection)));
                    WriteBodyResponse(ctx, 200, "OK", returnConstructor.ToString());
                }
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private JsonDictionaryStringConstructor WriteJoinRequestToOutput(JoinRequest requestIn, MySqlDataManipulator connection)
        {
            JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
            ret.SetMapping("Id", requestIn.Id);
            var user = connection.GetUserById(requestIn.UserId);
            if(user == null)
            {
                ret.SetMapping("DisplayName", "Unknown");
                ret.SetMapping("Email", "Unknown");
            }
            else
            {
                var userSettings = JsonDataObjectUtil<List<SettingsEntry>>.ParseObject(user.Settings);
                ret.SetMapping("DisplayName", userSettings.Where(entry => entry.Key.Equals("displayName")).First().Value);
                ret.SetMapping("Email", user.Email);
            }
            return ret;
        }

        /// <summary>
        /// DELETE request format located in the Web Api Enumeration v2
        /// under the tab Company/Requests, starting row 73
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
                CompanyRequestsPatchRequest entry = JsonDataObjectUtil<CompanyRequestsPatchRequest>.ParseObject(ctx);
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
                    if ((mappedUser.AccessLevel & AccessLevelMasks.AdminMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not an administrative user");
                        return;
                    }
                    var request = connection.GetJoinRequestById(mappedUser.Company, entry.RequestId);
                    if (request == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "Request was not found on the server");
                        return;
                    }
                    if (!connection.RemoveJoinRequest(mappedUser.Company, entry.RequestId, accept: false))
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error ocurred on the server: " + connection.LastException);
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
        /// under the tab Company/Requests, starting row 49
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
                string requestStr = new StreamReader(ctx.Request.InputStream).ReadToEnd();
                CompanyRequestsPutRequest entry = JsonDataObjectUtil<CompanyRequestsPutRequest>.ParseObject(requestStr);
                if (!ValidatePutRequest(entry))
                {
                    CompanyRequestsGetRequest req2 = JsonDataObjectUtil<CompanyRequestsGetRequest>.ParseObject(requestStr);
                    if(req2 != null && ValidateGetRequest(req2))
                    {
                        HandleGetRequest(ctx, req2);
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
                    if ((mappedUser.AccessLevel & AccessLevelMasks.AdminMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not an administrative user");
                        return;
                    }
                    var request = connection.GetJoinRequestById(mappedUser.Company, entry.RequestId);
                    if(request == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "Request was not found on the server");
                        return;
                    }
                    if(!connection.RemoveJoinRequest(mappedUser.Company, entry.RequestId, accept: true))
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error ocurred on the server: " + connection.LastException);
                        return;
                    }
                    mappedUser.AccessLevel = 1;
                    connection.UpdateUserAccessLevel(mappedUser);
                    WriteBodylessResponse(ctx, 200, "OK");
                }
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private bool ValidatePutRequest(CompanyRequestsPutRequest req)
        {
            if (req.LoginToken == null || req.LoginToken.Equals(""))
                return false;
            if (req.AuthToken == null || req.AuthToken.Equals(""))
                return false;
            if (req.RequestId <= 0)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }

        private bool ValidatePatchRequest(CompanyRequestsPatchRequest req)
        {
            if (req.LoginToken == null || req.LoginToken.Equals(""))
                return false;
            if (req.AuthToken == null || req.AuthToken.Equals(""))
                return false;
            if (req.RequestId <= 0)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }

        private bool ValidateGetRequest(CompanyRequestsGetRequest req)
        {
            if (req.LoginToken == null || req.LoginToken.Equals(""))
                return false;
            if (req.AuthToken == null || req.AuthToken.Equals(""))
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }
    }
}

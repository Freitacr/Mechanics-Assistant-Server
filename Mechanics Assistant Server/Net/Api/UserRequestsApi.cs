﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;
using System.IO;

namespace OldManInTheShopServer.Net.Api
{
    [DataContract]
    class UserRequestsGetRequest
    {
        [DataMember]
        public int UserId = default;

        [DataMember]
        public string LoginToken = default;
    }

    class UserRequestsApi : ApiDefinition
    {
#if RELEASE
        public UserRequestsApi(int portIn) : base("https://+:" + portIn + "/user/requests")
#elif DEBUG
        public UserRequestsApi(int portIn) : base("http://+:" + portIn + "/user/requests")
#endif
        {
            PUT += HandlePutRequest;
        }

        /// <summary>
        /// Request for retrieving a user's previous request history. Documention is found in the Web API Enumeration file
        /// in the /User/Request tab, starting at row 1
        /// </summary>
        /// <param name="ctx">The HttpListenerContext to respond to</param>
        private void HandlePutRequest(HttpListenerContext ctx)
        {
            try
            {
                #region Input Validation
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "No Body", "Request lacked a body");
                    return;
                }
                UserRequestsGetRequest req = JsonDataObjectUtil<UserRequestsGetRequest>.ParseObject(ctx);
                if (req == null)
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Request was in the wrong format");
                    return;
                }
                if (!ValidateRequest(req))
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Not all fields of the request were filled");
                    return;
                }
                #endregion

                MySqlDataManipulator connection = new MySqlDataManipulator();
                using (connection)
                {
                    bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected ServerError", "Connection to database failed");
                        return;
                    }
                    #region User Validation
                    var user = connection.GetUserById(req.UserId);
                    if (user == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(user, req.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Unauthorized", "Login Token was expired or incorrect");
                        return;
                    }
                    #endregion

                    #region Action Handling
                    List<PreviousUserRequest> requestHistory = user.DecodeRequests();
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<PreviousUserRequest>));
                    MemoryStream streamOut = new MemoryStream();
                    serializer.WriteObject(streamOut, requestHistory);
                    byte[] requestHistoryBytes = streamOut.ToArray();
                    string requestHistoryString = Encoding.UTF8.GetString(requestHistoryBytes);
                    WriteBodyResponse(ctx, 200, "OK", requestHistoryString);
                    #endregion

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
    
        public bool ValidateRequest(UserRequestsGetRequest req)
        {
            if (req.UserId <= 0)
                return false;
            return !(req.LoginToken == null || req.LoginToken.Equals("") || req.LoginToken.Equals("x''"));
        }
    }

}

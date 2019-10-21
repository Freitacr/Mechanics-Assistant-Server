﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using MechanicsAssistantServer.Data.MySql.TableDataTypes;
using MechanicsAssistantServer.Data.MySql;
using MechanicsAssistantServer.Util;
using System.IO;

namespace MechanicsAssistantServer.Net.Api
{

    [DataContract]
    class UserSettingsEditRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;

        [DataMember]
        public string Key;

        [DataMember]
        public string Value;
    }

    [DataContract]
    class UserSettingsGetRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;
    }

    class UserSettingsApi : ApiDefinition
    {
#if RELEASE
        public UserSettingsApi(int portIn) : base("https://+:" + portIn + "/user/settings")
#elif DEBUG
        public UserSettingsApi(int portIn) : base("http://+:" + portIn + "/user/settings")
#endif        
        {
            GET += HandleGetRequest;
            PATCH += HandlePatchRequest;
        }

        private void HandleGetRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "No Body", "Request lacked a body");
                    return;
                }
                UserSettingsGetRequest req = JsonDataObjectUtil<UserSettingsGetRequest>.ParseObject(ctx);
                if (req == null)
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Request was in the wrong format");
                    return;
                }
                if (!ValidateGetRequest(req))
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Not all fields of the request were filled");
                    return;
                }
                MySqlDataManipulator connection = new MySqlDataManipulator();
                using (connection)
                {
                    bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected ServerError", "Connection to database failed");
                        return;
                    }
                    var user = connection.GetUserById(req.UserId);
                    if (user == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(user, req.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Unauthorized", "Email or password was incorrect");
                        return;
                    }
                    WriteBodyResponse(ctx, 200, "OK", user.Settings, "application/json");
                }
            } catch(Exception e)
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
                    WriteBodyResponse(ctx, 400, "No Body", "Request lacked a body");
                    return;
                }
                UserSettingsEditRequest req = JsonDataObjectUtil<UserSettingsEditRequest>.ParseObject(ctx);
                if (req == null)
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Request was in the wrong format");
                    return;
                }
                if (!ValidateEditRequest(req))
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Not all fields of the request were filled");
                    return;
                }
                MySqlDataManipulator connection = new MySqlDataManipulator();
                using (connection)
                {
                    bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected ServerError", "Connection to database failed");
                        return;
                    }
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
                    if (!UserVerificationUtil.AuthTokenValid(user, req.AuthToken))
                    {
                        WriteBodyResponse(ctx, 401, "Unauthorized", "Auth Token was expired or incorrect");
                        return;
                    }
                    if (!user.UpdateSettings(req.Key, req.Value))
                    {
                        WriteBodyResponse(ctx, 404, "NotFound", "Setting with key " + req.Key + " was not found.");
                        return;
                    }
                    if (!connection.UpdateUsersSettings(user))
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected Server Error", "Exception: " + connection.LastException.Message);
                        return;
                    }
                    WriteBodylessResponse(ctx, 200, "OK");
                }
            } catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private bool ValidateGetRequest(UserSettingsGetRequest req)
        {
            return !(req.LoginToken == null || req.LoginToken.Equals("") || req.LoginToken.Equals("0x"));
        }

        private bool ValidateEditRequest(UserSettingsEditRequest req)
        {
            if (req.Key == null || req.Key.Equals(""))
                return false;
            if (req.Value == null || req.Value.Equals(""))
                return false;
            if (req.AuthToken == null || req.AuthToken.Equals("") || req.AuthToken.Equals("0x"))
                return false;
            return !(req.LoginToken == null || req.LoginToken.Equals("") || req.LoginToken.Equals("0x"));
        }
    }
}

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
    class UserJobDeleteRequest
    {
        [DataMember]
        public string AuthToken;
        [DataMember]
        public string LoginToken;
        [DataMember]
        public int UserId;
        [DataMember]
        public int JobId;
    }

    class UserJobApi : ApiDefinition
    {

        public UserJobApi(int portIn) : base("https://+:"+portIn+"/user/job")
        {
            DELETE += HandleDeleteRequest;
        }

        private void HandleDeleteRequest(HttpListenerContext ctx)
        {
            if (!ctx.Request.HasEntityBody)
            {
                WriteBodyResponse(ctx, 400, "No Body", "Request lacked a body");
                return;
            }
            UserJobDeleteRequest req = JsonDataObjectUtil<UserJobDeleteRequest>.ParseObject(ctx);
            if (req == null)
            {
                WriteBodyResponse(ctx, 400, "Incorrect Format", "Request was in the wrong format");
                return;
            }
            if (!ValidateDeletionRequest(req))
            {
                WriteBodyResponse(ctx, 400, "Incorrect Format", "Not all fields of the request were filled");
                return;
            }
            MySqlDataManipulator connection = new MySqlDataManipulator();
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
            if(!connection.DeleteUserJobData(user, req.JobId))
            {
                WriteBodyResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
                return;
            }
            WriteBodylessResponse(ctx, 200, "OK");
            connection.Close();
        }

        private bool ValidateDeletionRequest(UserJobDeleteRequest req)
        {
            if (req.AuthToken.Equals("") || req.AuthToken.Equals("0x"))
                return false;
            if (req.LoginToken.Equals("") || req.LoginToken.Equals("0x"))
                return false;
            return !(req.JobId <= 0 || req.JobId >= 3);
        }
    }
}
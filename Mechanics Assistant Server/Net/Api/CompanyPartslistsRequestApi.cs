﻿using System;
using System.Collections.Generic;
using System.Text;
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
                if (ValidateFullPostRequest(entry))
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
            if (req.LoginToken == "")
                return false;
            if (req.AuthToken == "")
                return false;
            if (req.RequiredPartsList == "")
                return false;
            return true;
        }
        private void HandleGetRequest(HttpListenerContext ctx)
        {

        }

        private void HandleDeleteRequest(HttpListenerContext ctx)
        {

        }

        private void HandlePatchRequest(HttpListenerContext ctx)
        {

        }
        private void HandlePutRequest(HttpListenerContext ctx)
        {

        }
    }
}

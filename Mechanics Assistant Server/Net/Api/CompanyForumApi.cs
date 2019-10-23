﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Net;
using OldManinTheShopServer.Data.MySql.TableDataTypes;
using OldManinTheShopServer.Data.MySql;
using OldManinTheShopServer.Util;
namespace OldManinTheShopServer.Net.Api
{
    [DataContract]
    class CompanyForumApiFullPostRequest
    {
        [DataMember]
        public int UserID;
        [DataMember]
        public string LoginToken;
        [DataMember]
        public string PostText;
        [DataMember]
        public int CompanyID;
        [DataMember]
        public int JobEntryID;
        [DataMember]
        public string AuthToken;
    }

    class CompanyForumApi : ApiDefinition
    {
#if RELEASE
        public CompanyForumApi(int port) : base("https://+:" + port + "/company/forum")
#elif DEBUG
        public CompanyForumApi(int port) : base("http://+:" + port + "/company/forum")
#endif
        {
            POST += HandlePostRequest;
            GET += HandleGetRequest;
            DELETE += HandleDeleteRequest;
        }

        private void HandlePostRequest(HttpListenerContext ctx)
        {
            if (!ctx.Request.HasEntityBody)
            {
                WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                return;
            }
            CompanyForumApiFullPostRequest entry = ParseForumEntry(ctx);
            if (!ValidateFullPostRequest(entry))
            {
                WriteBodyResponse(ctx, 400, "Bad Request", "Incorrect Format");
                return;
            }
            //Otherwise we have a valid entry, validate user
            MySqlDataManipulator connection = new MySqlDataManipulator();
            bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
            if (!res)
            {
                WriteBodyResponse(ctx, 500, "Unexpected ServerError", "Connection to database failed");
                return;
            }
            OverallUser mappedUser = connection.GetUserById(entry.UserID);
            if (!UserVerificationUtil.LoginTokenValid(mappedUser, entry.LoginToken))
            {
                WriteBodyResponse(ctx,401,"Not Authorized","Login token was incorrect.");
                return;
            }
            //user is good, add post text
            res = connection.AddForumPosting(entry.CompanyID,entry.UserID,entry.PostText);
            if (!res)
            {
                WriteBodyResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
                return;
            }
            WriteBodylessResponse(ctx, 200, "OK");
            connection.Close();
        }

        private CompanyForumApiFullPostRequest ParseForumEntry(HttpListenerContext ctx)
        {
            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(CompanyForumApiFullPostRequest));
            try
            {
                try
                {
                    return (CompanyForumApiFullPostRequest)deserializer.ReadObject(ctx.Request.InputStream);
                } catch (InvalidCastException)
                {
                    return null;
                }
            }
            catch (SerializationException)
            {
                return null;
            }
        }
        private bool ValidateFullPostRequest(CompanyForumApiFullPostRequest req)
        {
            if (req.UserID == -1)
                return false;
            if (req.LoginToken == "")
                return false;
            if (req.PostText == "")
                return false;
            if (req.CompanyID == -1)
                return false;
            if (req.JobEntryID == -1)
                return false;
            return true;
        }
        private void HandleGetRequest(HttpListenerContext ctx)
        {

        }
        private void HandleDeleteRequest(HttpListenerContext ctx)
        {

        }

    }
}
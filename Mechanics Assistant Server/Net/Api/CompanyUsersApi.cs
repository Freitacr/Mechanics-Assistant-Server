﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Serialization;
using System.Net;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Net.Api
{
    [DataContract]
    class CompanyUsersApiGetRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;
    }


    class CompanyUsersApi : ApiDefinition
    {
#if RELEASE
        public CompanyUsersApi(int port) : base("https://+:"+port+"/company/users")
#elif DEBUG
        public CompanyUsersApi(int port) : base("http://+:"+port+"/company/users")
#endif
        {
            PUT += HandlePutRequest;
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
                CompanyUsersApiGetRequest entry = JsonDataObjectUtil<CompanyUsersApiGetRequest>.ParseObject(ctx);
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
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Auth token was incorrect.");
                        return;
                    }
                    if ((mappedUser.AccessLevel & AccessLevelMasks.AdminMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not an admin");
                        return;
                    }
                    List<OverallUser> companyUsers = connection.GetUsersWhere("Company=" + mappedUser.Company);
                    JsonListStringConstructor retConstructor = new JsonListStringConstructor();
                    companyUsers.ForEach(user => retConstructor.AddElement(ConvertUserToOutput(user)));
                    WriteBodyResponse(ctx, 200, "OK", retConstructor.ToString());
                }
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private JsonDictionaryStringConstructor ConvertUserToOutput(OverallUser toConvert)
        {
            JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
            ret.SetMapping("Access Level", toConvert.AccessLevel);
            ret.SetMapping("Email", toConvert.Email);
            ret.SetMapping("DatabaseId", toConvert.UserId);
            List<SettingsEntry> entries = JsonDataObjectUtil<List<SettingsEntry>>.ParseObject(toConvert.Settings);
            ret.SetMapping("Display Name", entries.Where(obj => obj.Key.Equals("displayName")).First().Value);
            return ret;
        }

        private bool ValidateGetRequest(CompanyUsersApiGetRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.AuthToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }
    }
}
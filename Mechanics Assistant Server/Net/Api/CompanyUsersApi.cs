using System;
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
        public int UserId = default;

        [DataMember]
        public string LoginToken = default;

        [DataMember]
        public string AuthToken = default;
    }

    [DataContract]
    class CompanyUsersApiPatchRequest
    {
        [DataMember]
        public int UserId = default;

        [DataMember]
        public string LoginToken = default;

        [DataMember]
        public string AuthToken = default;

        [DataMember]
        public int CompanyUserId = default;

        [DataMember]
        public int AccessLevel = default;
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
            PATCH += HandlePatchRequest;
        }

        private void HandlePutRequest(HttpListenerContext ctx)
        {
            try
            {
                #region Input Validation
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
                #endregion

                MySqlDataManipulator connection = new MySqlDataManipulator();
                using (connection)
                {
                    bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected Server Error", "Connection to database failed");
                        return;
                    }
                    #region User Validation
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
                    #endregion

                    #region Action Handling
                    List<OverallUser> companyUsers = connection.GetUsersWhere("Company=" + mappedUser.Company);
                    JsonListStringConstructor retConstructor = new JsonListStringConstructor();
                    companyUsers.ForEach(user => retConstructor.AddElement(ConvertUserToOutput(user)));
                    WriteBodyResponse(ctx, 200, "OK", retConstructor.ToString());
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

        private JsonDictionaryStringConstructor ConvertUserToOutput(OverallUser toConvert)
        {
            JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
            ret.SetMapping("Access Level", toConvert.AccessLevel);
            ret.SetMapping("Email", toConvert.Email);
            ret.SetMapping("DatabaseId", toConvert.UserId);
            List<UserSettingsEntry> entries = JsonDataObjectUtil<List<UserSettingsEntry>>.ParseObject(toConvert.Settings);
            ret.SetMapping("Display Name", entries.Where(obj => obj.Key.Equals(UserSettingsEntryKeys.DisplayName)).First().Value);
            return ret;
        }

        private void HandlePatchRequest(HttpListenerContext ctx)
        {
            try
            {
                #region Input Validation
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                CompanyUsersApiPatchRequest entry = JsonDataObjectUtil<CompanyUsersApiPatchRequest>.ParseObject(ctx);
                if (!ValidatePatchRequset(entry))
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "Incorrect Format");
                    return;
                }
                #endregion

                MySqlDataManipulator connection = new MySqlDataManipulator();
                using (connection)
                {
                    bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected Server Error", "Connection to database failed");
                        return;
                    }
                    #region User Validation
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
                    #endregion

                    #region Action Handling
                    OverallUser user = connection.GetUserById(entry.CompanyUserId);
                    if(user == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "Company User was not found on the server");
                        return;
                    }
                    user.AccessLevel = entry.AccessLevel;
                    if(!connection.UpdateUserAccessLevel(user))
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occurred while updating user's access level: " + connection.LastException.Message);
                        return;
                    }
                    WriteBodylessResponse(ctx, 200, "OK");
                    #endregion
                }
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private bool ValidatePatchRequset(CompanyUsersApiPatchRequest req)
        {
            if (req.UserId <= 0)
                return false;
            if (req.CompanyUserId <= 0)
                return false;
            if (req.AuthToken == null)
                return false;
            if (req.LoginToken == null)
                return false;
            if ((req.AccessLevel & AccessLevelMasks.MechanicMask) != 0 && req.AccessLevel > -1 && req.AccessLevel < 16)
                return true;
            return false;
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

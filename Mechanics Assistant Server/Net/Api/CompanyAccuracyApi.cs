using System;
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
    class CompanyAccuracyApiFullGetRequest
    {
        [DataMember]
        public int UserId;
        [DataMember]
        public string LoginToken;
    }
    class CompanyAccuracyApi : ApiDefinition
    { 
#if RELEASE
        public CompanyAccuracyApi(int port) : base("https://+:" + port + "/company/accuracy")
#elif DEBUG
        public CompanyAccuracyApi(int port) : base("http://+:" + port + "/company/accuracy")
#endif
        {
            GET += HandleGetRequest;
        }
        private void HandleGetRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                CompanyAccuracyApiFullGetRequest entry = JsonDataObjectUtil<CompanyAccuracyApiFullGetRequest>.ParseObject(ctx);
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
                    if((mappedUser.AccessLevel & AccessLevelMasks.AdminMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "User was not an admin");
                        return;
                    }
                    double companyAccuracy = connection.GetCompanyAccuracy(mappedUser.Company);

                    
                    WriteBodyResponse(ctx, 200, "OK", companyAccuracy.ToString());
                }
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }

        }
        private bool ValidateGetRequest(CompanyAccuracyApiFullGetRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }
    }
}

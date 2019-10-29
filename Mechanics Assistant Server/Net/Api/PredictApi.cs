using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Net;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;
using OldManInTheShopServer.Models;

namespace OldManInTheShopServer.Net.Api
{
    [DataContract]
    class PredictApiGetRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public JobDataEntry Entry;

        [DataMember]
        public int ComplaintGroupId;

        [DataMember]
        public int CompanyId;
    }

    [DataContract]
    class PredictApiPutRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public JobDataEntry Entry;

        [DataMember]
        public int CompanyId;
    }


    public class PredictApi : ApiDefinition
    {
#if RELEASE
        public PredictApi(int port) : base("https://+:" + port + "/predict")
#elif DEBUG
        public PredictApi(int port) : base("https://+:" + port + "/predict")
#endif
        {
            GET += HandleGetRequest;
            PUT += HandlePutRequest;
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
                PredictApiGetRequest req = JsonDataObjectUtil<PredictApiGetRequest>.ParseObject(ctx);
                if (!ValidateGetRequest(req))
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
                    OverallUser mappedUser = connection.GetUserById(req.UserId);
                    if (mappedUser == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(mappedUser, req.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Login token was incorrect.");
                        return;
                    }
                    if (mappedUser.Company != req.CompanyId)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Cannot predict using other company's data");
                        return;
                    }
                    DatabaseQueryProcessor processor = new DatabaseQueryProcessor();
                    string ret = processor.ProcessQueryForSimilaryQueries(req.Entry, connection, req.CompanyId, req.ComplaintGroupId, 10);
                    WriteBodyResponse(ctx, 200, "OK", ret, "application/json");
                }
            } 
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occurred during processing of request: " + e.Message);
            }
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
                PredictApiPutRequest req = JsonDataObjectUtil<PredictApiPutRequest>.ParseObject(ctx);
                if(!ValidatePutRequest(req))
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
                    OverallUser mappedUser = connection.GetUserById(req.UserId);
                    if (mappedUser == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(mappedUser, req.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Login token was incorrect.");
                        return;
                    }
                    if(mappedUser.Company != req.CompanyId)
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Cannot predict using other company's data");
                        return;
                    }
                    DatabaseQueryProcessor processor = new DatabaseQueryProcessor();
                    string ret = processor.ProcessQueryForComplaintGroups(req.Entry, connection, req.CompanyId);
                    WriteBodyResponse(ctx, 200, "OK", ret, "application/json");
                }
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occurred during processing of request: " + e.Message);
            }
        }

        private bool ValidateGetRequest(PredictApiGetRequest req)
        {
            if (req.Entry == null)
                return false;
            if (req.LoginToken == null)
                return false;
            if (req.CompanyId <= 0)
                return false;
            if (req.ComplaintGroupId < 0)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }

        private bool ValidatePutRequest(PredictApiPutRequest req)
        {
            if (req.Entry == null)
                return false;
            if (req.LoginToken == null)
                return false;
            if (req.CompanyId <= 0)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }
    }
}

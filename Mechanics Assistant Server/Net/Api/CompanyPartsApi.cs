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
    class CompanyPartsApiFullPostRequest
    {
        [DataMember]
        public int UserId;
        [DataMember]
        public string LoginToken;
        [DataMember]
        public string AuthToken;
        [DataMember]
        public string Make;
        [DataMember]
        public string Model;
        [DataMember]
        public int Year;
        [DataMember]
        public string PartId;
        [DataMember]
        public string PartName;
        [DataMember]
        public int CompanyId;
    }
    class CompanyPartsApi : ApiDefinition
    {
#if RELEASE
        public CompanyPartsApi(int port) : base("https://+:" + port + "/company/parts")
#elif DEBUG
        public CompanyPartsApi(int port) : base("http://+:" + port + "/company/parts")
#endif
        {
            POST += HandlePostRequest;
            GET += HandleGetRequest;
            DELETE += HandleDeleteRequest;
            PATCH += HandlePatchRequest;
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
                CompanyPartsApiFullPostRequest entry = JsonDataObjectUtil<CompanyPartsApiFullPostRequest>.ParseObject(ctx);
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
                        WriteBodyResponse(ctx,500,"Unexpected Server Error","Connection to database failed");
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
                        WriteBodyResponse(ctx,401,"Not Authorized","Login token was incorrect.");
                        return;
                    }
                    if (!UserVerificationUtil.AuthTokenValid(mappedUser, entry.AuthToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Auth token was ezpired or incorrect");
                        return;
                    }
                    if ((mappedUser.AccessLevel & AccessLevelMasks.PartMask) == 0)
                    {
                        WriteBodyResponse(ctx,401,"Not Autherized","Not marked as a Parts User");
                        return;
                    }
                    PartCatalogueEntry ent = new PartCatalogueEntry(entry.Make,entry.Model,entry.Year,entry.PartId,entry.PartName);
                    res = connection.AddPartEntry(entry.CompanyId, ent);
                    if (!res)
                    {
                        WriteBodyResponse(ctx,500,"Unexpected Server Error",connection.LastException.Message);
                        return;
                    }
                    WriteBodylessResponse(ctx, 200, "OK");
                }
            } catch(Exception e)
            {
                WriteBodyResponse(ctx,500,"Internal Server Error",e.Message);
            }
        }
        private bool ValidateFullPostRequest(CompanyPartsApiFullPostRequest req)
        {

            if (req.UserId == -1)
                return false;
            if (req.LoginToken == null || req.LoginToken.Equals(""))
                return false;
            if (req.AuthToken == null || req.AuthToken.Equals(""))
                return false;
            if (req.Make == null || req.Make.Equals(""))
                return false;
            if (req.Year == -1)
                return false;
            if (req.Model == null || req.Model.Equals(""))
                return false;
            if (req.PartId == null || req.PartId.Equals(""))
                return false;
            if (req.PartName == null || req.PartName.Equals(""))
                return false;
            if (req.CompanyId == -1)
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
    }
}

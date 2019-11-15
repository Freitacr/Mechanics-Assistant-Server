using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Net;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;
using System.IO;

namespace OldManInTheShopServer.Net.Api
{
    [DataContract]
    class CompanyListRetrieveRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string NamePortion;
    }

    [DataContract]
    class UsableCompanyListRetrieveRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;
    }

    class CompanyListApi : ApiDefinition
    {
#if RELEASE
        public CompanyListApi(int port) : base("https://+:" + port + "/company/list")
#elif DEBUG
        public CompanyListApi(int port) : base("http://+:" + port + "/company/list")
#endif
        {
            PUT += HandlePutRequest;
            POST += HandlePostRequest;
        }

        private void HandlePostRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Invalid Format", "Request did not contain a body");
                    return;
                }
                UsableCompanyListRetrieveRequest entry = JsonDataObjectUtil<UsableCompanyListRetrieveRequest>.ParseObject(ctx);
                if (entry == null)
                {
                    WriteBodylessResponse(ctx, 400, "Invalid Format");
                    return;
                }
                if (!ValidateUsableRetrieveRequest(entry))
                {
                    WriteBodyResponse(ctx, 400, "Invalid Format", "One or more fields contained an invalid value or were missing");
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
                    List<CompanyId> companies = connection.GetPublicCompanies();
                    CompanyId userCompany = connection.GetCompanyById(mappedUser.Company);
                    if (companies == null)
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occured while retrieving companies: " + connection.LastException.Message);
                        return;
                    }
                    if (!companies.Contains(userCompany))
                        companies.Add(userCompany);
                    JsonListStringConstructor retConstructor = new JsonListStringConstructor();
                    companies.ForEach(req => retConstructor.AddElement(WriteCompanyIdToOutput(req)));

                    WriteBodyResponse(ctx, 200, "OK", retConstructor.ToString());
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

        private void HandlePutRequest(HttpListenerContext ctx)
        {
            try
            {
                if(!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Invalid Format", "Request did not contain a body");
                    return;
                }
                CompanyListRetrieveRequest entry = JsonDataObjectUtil<CompanyListRetrieveRequest>.ParseObject(ctx);
                if(entry == null)
                {
                    WriteBodylessResponse(ctx, 400, "Invalid Format");
                    return;
                }
                if (!ValidateRetrieveRequest(entry))
                {
                    WriteBodyResponse(ctx, 400, "Invalid Format", "One or more fields contained an invalid value or were missing");
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
                    if(entry.NamePortion.Contains('`'))
                    {
                        WriteBodyResponse(ctx, 400, "Bad Request", "Request contained a backtick character(`)." +
                            "This character is disallowed due to SQL injection attacks.");
                        return;
                    }
                    List<CompanyId> companies = connection.GetCompaniesWithNamePortion(entry.NamePortion);
                    if(companies == null)
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occured while retrieving companies: " + connection.LastException.Message);
                        return;
                    }
                    JsonListStringConstructor retConstructor = new JsonListStringConstructor();
                    companies.ForEach(req => retConstructor.AddElement(WriteCompanyIdToOutput(req)));

                    WriteBodyResponse(ctx, 200, "OK", retConstructor.ToString());
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
        
        private JsonDictionaryStringConstructor WriteCompanyIdToOutput(CompanyId idIn)
        {
            JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
            ret.SetMapping("Id", idIn.Id);
            ret.SetMapping("LegalName", idIn.LegalName);
            return ret;
        }

        private bool ValidateRetrieveRequest(CompanyListRetrieveRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            if (req.NamePortion == null || req.NamePortion.Equals(""))
                return false;
            return true;
        }

        private bool ValidateUsableRetrieveRequest(UsableCompanyListRetrieveRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }
    }
}

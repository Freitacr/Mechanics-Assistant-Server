using System;
using System.Collections.Generic;
using System.Linq;
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
        public int UserId = default;
        [DataMember]
        public string LoginToken = default;
        [DataMember]
        public string AuthToken = default;
        [DataMember]
        public string Make = default;
        [DataMember]
        public string Model = default;
        [DataMember]
        public int Year = default;
        [DataMember]
        public string PartId = default;
        [DataMember]
        public string PartName = default;
        [DataMember]
        public int CompanyId = default;
    }

    [DataContract]
    class CompanyPartsApiGetRequest
    {
        [DataMember]
        public int UserId = default;

        [DataMember]
        public string LoginToken = default;

        [DataMember]
        public string AuthToken = default;

        [DataMember]
        public int StartRange = default;

        [DataMember]
        public int EndRange = default;
    }

    [DataContract]
    class CompanyPartsApiDeleteRequest
    {
        [DataMember]
        public int UserId = default;

        [DataMember]
        public string LoginToken = default;

        [DataMember]
        public string AuthToken = default;

        [DataMember]
        public int PartEntryId = default;
    }

    [DataContract]
    class CompanyPartsApiPatchRequest
    {
        [DataMember]
        public int UserId = default;

        [DataMember]
        public string LoginToken = default;

        [DataMember]
        public string AuthToken = default;

        [DataMember]
        public string FieldName = default;

        [DataMember]
        public string FieldValue = default;

        [DataMember]
        public int PartEntryId = default;
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

        /// <summary>
        /// POST request format located in the Web Api Enumeration v2
        /// under the tab Company/Parts, starting row 1
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
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
            }
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
            }
            catch (Exception e)
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

        /// <summary>
        /// GET request format located in the Web Api Enumeration v2
        /// under the tab Company/Parts, starting row 72
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandleGetRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                CompanyPartsApiGetRequest entry = JsonDataObjectUtil<CompanyPartsApiGetRequest>.ParseObject(ctx);
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
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Auth token was ezpired or incorrect");
                        return;
                    }
                    if ((mappedUser.AccessLevel & AccessLevelMasks.PartMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Autherized", "Not marked as a Parts User");
                        return;
                    }

                    List<PartCatalogueEntry> catelogue = connection.GetPartCatalogueEntries(mappedUser.Company);
                    JsonListStringConstructor retConstructor = new JsonListStringConstructor();
                    catelogue.ForEach(part => retConstructor.AddElement(WritePartCatelogueEntryToOutput(part)));

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

        private JsonDictionaryStringConstructor WritePartCatelogueEntryToOutput(PartCatalogueEntry entryOut)
        {
            JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
            ret.SetMapping("Make", entryOut.Make);
            ret.SetMapping("Model", entryOut.Model);
            if (entryOut.Year == -1)
                ret.SetMapping("Year", "Unknown");
            else
                ret.SetMapping("Year", entryOut.Year);
            ret.SetMapping("PartId", entryOut.PartId);
            ret.SetMapping("PartName", entryOut.PartName);
            return ret;
        }

        /// <summary>
        /// DELETE request format located in the Web Api Enumeration v2
        /// under the tab Company/Parts, starting row 51
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandleDeleteRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                CompanyPartsApiDeleteRequest entry = JsonDataObjectUtil<CompanyPartsApiDeleteRequest>.ParseObject(ctx);
                if (!ValidateDeleteRequest(entry))
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
                    if ((mappedUser.AccessLevel & AccessLevelMasks.PartMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Autherized", "Not marked as a Parts User");
                        return;
                    }

                    if(connection.GetPartCatalogueEntryById(mappedUser.Company, entry.PartEntryId) == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "Part entry with the given id was not found");
                        return;
                    }

                    if(!connection.RemovePartCatalogueEntry(mappedUser.Company, entry.PartEntryId))
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occured while removing part entry: " + connection.LastException.Message);
                        return;
                    }
                    WriteBodylessResponse(ctx, 200, "OK");
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


        /// <summary>
        /// PATCH request format located in the Web Api Enumeration v2
        /// under the tab Company/Parts, starting row 28
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandlePatchRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                CompanyPartsApiPatchRequest entry = JsonDataObjectUtil<CompanyPartsApiPatchRequest>.ParseObject(ctx);
                if (!ValidatePatchRequest(entry))
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
                    if ((mappedUser.AccessLevel & AccessLevelMasks.PartMask) == 0)
                    {
                        WriteBodyResponse(ctx, 401, "Not Autherized", "Not marked as a Parts User");
                        return;
                    }

                    var partEntry = connection.GetPartCatalogueEntryById(mappedUser.Company, entry.PartEntryId);

                    if (partEntry == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "Part entry with the given id was not found");
                        return;
                    }

                    switch (entry.FieldName)
                    {
                        case "Make":
                            partEntry.Make = entry.FieldValue;
                            break;
                        case "Model":
                            partEntry.Model = entry.FieldValue;
                            break;
                        case "Year":
                            partEntry.Year = int.Parse(entry.FieldValue);
                            break;
                        case "PartId":
                            partEntry.PartId = entry.FieldValue;
                            break;
                        case "PartName":
                            partEntry.PartName = entry.FieldValue;
                            break;
                        default:
                            WriteBodyResponse(ctx, 404, "Not Found", "The field specified was not found");
                            return;
                    }

                    if (!connection.UpdatePartEntry(mappedUser.Company, partEntry))
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occured while removing part entry: " + connection.LastException.Message);
                        return;
                    }
                    WriteBodylessResponse(ctx, 200, "OK");
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


        private bool ValidateGetRequest(CompanyPartsApiGetRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.AuthToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            if (req.StartRange <= 0)
                return false;
            if (req.EndRange <= 1)
                return false;
            return true;
        }

        private bool ValidateDeleteRequest(CompanyPartsApiDeleteRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.AuthToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            if (req.PartEntryId <= 0)
                return false;
            return true;
        }

        private bool ValidatePatchRequest(CompanyPartsApiPatchRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.AuthToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            if (req.PartEntryId <= 0)
                return false;
            if (req.FieldName == null)
                return false;
            if (req.FieldValue == null || req.FieldValue.Equals(""))
                return false;
            return true;
        }
    }
}

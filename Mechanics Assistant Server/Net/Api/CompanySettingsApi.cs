﻿using System;
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
    class CompanySettingsApiPatchRequest
    {
        [DataMember]
        public int UserId;
        [DataMember]
        public string LoginToken;
        [DataMember]
        public string AuthToken;
        [DataMember]
        public string SettingsValue;
        [DataMember]
        public string SettingsKey;
    }

    [DataContract]
    class CompanySettingsApiPutRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;
    }

    class CompanySettingsApi : ApiDefinition
    {
#if RELEASE
        public CompanySettingsApi(int port) : base("https://+:" + port + "/company/settings")
#elif DEBUG
        public CompanySettingsApi(int port) : base("http://+:" + port + "/company/settings")
#endif
        {
            PUT += HandlePutRequest;
            PATCH += HandlePatchRequest;
        }

        /// <summary>
        /// GET request format located in the Web Api Enumeration v2
        /// under the tab Company/Settings, starting row 1
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandlePutRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "No Body", "Request lacked a body");
                    return;
                }
                CompanySettingsApiPutRequest entry = JsonDataObjectUtil<CompanySettingsApiPutRequest>.ParseObject(ctx);
                if (entry == null)
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Request was in the wrong format");
                    return;
                }
                if (!ValidatePutRequest(entry))
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Not all fields in the request were filled");
                    return;
                }
                MySqlDataManipulator connection = new MySqlDataManipulator();
                using (connection)
                {
                    bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected ServerError", "Connection to database failed");
                        return;
                    }
                    var user = connection.GetUserById(entry.UserId);
                    if (user == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on the server");
                        return;
                    }
                    List<CompanySettingsEntry> entries = connection.GetCompanySettings(user.Company);
                    if (entries == null)
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occured while retrieving settings: " + connection.LastException.Message);
                        return;
                    }
                    JsonListStringConstructor retConstructor = new JsonListStringConstructor();
                    entries.ForEach(obj => retConstructor.AddElement(WriteSettingToOutput(obj)));
                    WriteBodyResponse(ctx, 200, "OK", retConstructor.ToString());
                }
            }
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occurred while processing request: " + e.Message);
            }
        }

        private JsonDictionaryStringConstructor WriteSettingToOutput(CompanySettingsEntry entry)
        {
            JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
            ret.SetMapping("SettingValue", entry.SettingValue);
            ret.SetMapping("SettingKey", entry.SettingKey);
            ret.SetMapping("Id", entry.Id);
            ret.SetMapping("Options", GetOptionsForKey(entry.SettingKey));
            return ret;
        }

        private JsonListStringConstructor GetOptionsForKey(string settingKey)
        {
            int keyHash = settingKey.GetHashCode();
            if (keyHash == CompanySettingsKey.Downvotes.GetHashCode())
            {
                return CompanySettingsOptions.Downvotes;
            } 
            else if (keyHash == CompanySettingsKey.KeywordClusterer.GetHashCode())
            {
                return CompanySettingsOptions.KeywordClusterer;
            }
            else if (keyHash == CompanySettingsKey.KeywordPredictor.GetHashCode())
            {
                return CompanySettingsOptions.KeywordPredictor;
            }
            else if (keyHash == CompanySettingsKey.ProblemPredictor.GetHashCode())
            {
                return CompanySettingsOptions.ProblemPredictor;
            }
            else if (keyHash == CompanySettingsKey.Public.GetHashCode())
            {
                return CompanySettingsOptions.Public;
            }
            else if (keyHash == CompanySettingsKey.RetrainInterval.GetHashCode())
            {
                return CompanySettingsOptions.RetrainInterval;
            } else
            {
                throw new ArgumentException("Setting with key " + settingKey + " did not have a listed set of options");
            }
        }

        /// <summary>
        /// PATCH request format located in the Web Api Enumeration v2
        /// under the tab Company/Settings, starting row 27
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandlePatchRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "No Body", "Request lacked a body");
                    return;
                }
                CompanySettingsApiPatchRequest entry = JsonDataObjectUtil<CompanySettingsApiPatchRequest>.ParseObject(ctx);
                if (entry == null)
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Request was in the wrong format");
                    return;
                }
                if (!ValidatePatchRequest(entry))
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Not all fields in the request were filled");
                    return;
                }
                MySqlDataManipulator connection = new MySqlDataManipulator();
                using (connection)
                {
                    bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected ServerError", "Connection to database failed");
                        return;
                    }
                    var user = connection.GetUserById(entry.UserId);
                    if (user == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on the server");
                        return;
                    }
                    List<CompanySettingsEntry> entries = connection.GetCompanySettings(user.Company);
                    if (entries == null)
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occured while retrieving settings: " + connection.LastException.Message);
                        return;
                    }

                    var toModify = entries.Where(obj => obj.SettingKey.Equals(entry.SettingsKey)).FirstOrDefault();
                    if(toModify == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "Setting with the specified key was not found on the server");
                        return;
                    }
                    toModify.SettingValue = entry.SettingsValue;
                    if(!connection.UpdateCompanySettings(user.Company, toModify))
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occurred while updating company's settings: " + connection.LastException.Message);
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
                WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occurred while processing request: " + e.Message);
            }
        }

        private bool ValidatePutRequest(CompanySettingsApiPutRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.AuthToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }

        private bool ValidatePatchRequest(CompanySettingsApiPatchRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.AuthToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            if (req.SettingsKey == null)
                return false;
            if (req.SettingsValue == null || req.SettingsValue.Equals(""))
                return false;
            return true;
        }
    }
}

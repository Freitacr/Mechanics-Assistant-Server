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
    class CompanyForumApiFullPostRequest
    {
        [DataMember]
        public int UserId;
        [DataMember]
        public string LoginToken;
        [DataMember]
        public string PostText;
        [DataMember]
        public int CompanyId;
        [DataMember]
        public int JobEntryId;
        [DataMember]
        public string AuthToken;
    }

    [DataContract]
    class CompanyForumApiGetRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public int JobEntryId;
    }

    [DataContract]
    class CompanyForumApiDeleteRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public int JobEntryId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;

        [DataMember]
        public int ForumPostId;
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

        /// <summary>
        /// POST request format located in the Web Api Enumeration v2
        /// under the tab Company/Forum, starting row 1
        /// </summary>
        /// <param name="ctx">HttpListenerContext to respond to</param>
        private void HandlePostRequest(HttpListenerContext ctx)
        {
            try {
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
                OverallUser mappedUser = connection.GetUserById(entry.UserId);
                if (!UserVerificationUtil.LoginTokenValid(mappedUser, entry.LoginToken))
                {
                    WriteBodyResponse(ctx, 401, "Not Authorized", "Login token was incorrect.");
                    return;
                }
                if(entry.PostText.Contains('<'))
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "Request contained the < character, which is disallowed due to cross site scripting attacks");
                    return;
                }
                //user is good, add post text
                res = connection.AddForumPost(entry.CompanyId, entry.UserId, new UserToTextEntry() { Text = entry.PostText, UserId = entry.UserId });
                if (!res)
                {
                    WriteBodyResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
                    return;
                }
                WriteBodylessResponse(ctx, 200, "OK");
                connection.Close();
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
            if (req.UserId == -1)
                return false;
            if (req.LoginToken == null || req.LoginToken.Equals(""))
                return false;
            if (req.PostText == null || req.LoginToken.Equals(""))
                return false;
            if (req.CompanyId == -1)
                return false;
            if (req.JobEntryId == -1)
                return false;
            return true;
        }

        /// <summary>
        /// GET request format located in the Web Api Enumeration v2
        /// under the tab Company/Forum, starting row 49
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

                CompanyForumApiGetRequest entry = JsonDataObjectUtil<CompanyForumApiGetRequest>.ParseObject(ctx);
                if (!ValidateGetRequest(entry))
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "Incorrect Format");
                    return;
                }
                //Otherwise we have a valid entry, validate user
                MySqlDataManipulator connection = new MySqlDataManipulator();
                using (connection)
                {
                    bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected ServerError", "Connection to database failed");
                        return;
                    }
                    OverallUser mappedUser = connection.GetUserById(entry.UserId);
                    if (mappedUser == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(mappedUser, entry.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Login token was incorrect.");
                        return;
                    }

                    JobDataEntry forumEntry = connection.GetDataEntryById(mappedUser.Company, entry.JobEntryId);
                    if(forumEntry == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "Job Data Entry was not found on the server");
                        return;
                    }
                    JsonListStringConstructor returnListConstructor = new JsonListStringConstructor();
                    JsonDictionaryStringConstructor repairJobConstructor = new JsonDictionaryStringConstructor();
                    repairJobConstructor.SetMapping("Make", forumEntry.Make);
                    repairJobConstructor.SetMapping("Model", forumEntry.Model);
                    if(forumEntry.Year == -1 )
                        repairJobConstructor.SetMapping("Year", "Unknown");
                    else
                        repairJobConstructor.SetMapping("Year", forumEntry.Year);
                    repairJobConstructor.SetMapping("Complaint", forumEntry.Complaint);
                    repairJobConstructor.SetMapping("Problem", forumEntry.Problem);
                    RequirementsEntry repairJobRequirements = RequirementsEntry.ParseJsonString(forumEntry.Requirements);
                    List<string> auxillaryRequirements = new List<string>(repairJobRequirements.Auxillary.Select(req => req.Requirement));
                    repairJobConstructor.SetMapping("AuxillaryRequirements", auxillaryRequirements);
                    repairJobConstructor.SetMapping("PartRequirements", repairJobRequirements.Parts);
                    repairJobConstructor.SetMapping("SafetyRequirements", repairJobRequirements.Safety);
                    returnListConstructor.AddElement(repairJobConstructor);

                    List<UserToTextEntry> forumPosts = connection.GetForumPosts(mappedUser.Company, entry.JobEntryId);
                    if (forumPosts == null)
                    {
                        WriteBodylessResponse(ctx, 404, "Not Found");
                        return;
                    }
                    forumPosts.ForEach(post => returnListConstructor.AddElement(ConvertForumPostToJson(post, connection)));
                    WriteBodyResponse(ctx, 200, "OK", returnListConstructor.ToString(), "application/json");
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

        private JsonDictionaryStringConstructor ConvertForumPostToJson(UserToTextEntry forumPost, MySqlDataManipulator connection)
        {
            JsonDictionaryStringConstructor retConstructor = new JsonDictionaryStringConstructor();
            var user = connection.GetUserById(forumPost.UserId);
            if(user == null)
            {
                retConstructor.SetMapping("DisplayName", "Unknown User");
            } else
            {
                List<UserSettingsEntry> userSettings = JsonDataObjectUtil<List<UserSettingsEntry>>.ParseObject(user.Settings);
                retConstructor.SetMapping("DisplayName", userSettings.Where(entry => entry.Key.Equals(UserSettingsEntryKeys.DisplayName)).First().Value);
            }
            retConstructor.SetMapping("PostText", forumPost.Text);
            retConstructor.SetMapping("ForumPostId", forumPost.Id);
            return retConstructor;
        }

        /// <summary>
        /// DELETE request format located in the Web Api Enumeration v2
        /// under the tab Company/Forum, starting row 26
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

                CompanyForumApiDeleteRequest entry = JsonDataObjectUtil<CompanyForumApiDeleteRequest>.ParseObject(ctx);
                if (!ValidateDeleteRequest(entry))
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "Incorrect Format");
                    return;
                }
                //Otherwise we have a valid entry, validate user
                MySqlDataManipulator connection = new MySqlDataManipulator();
                using (connection)
                {
                    bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected ServerError", "Connection to database failed");
                        return;
                    }
                    OverallUser mappedUser = connection.GetUserById(entry.UserId);
                    if(mappedUser == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(mappedUser, entry.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Login token was incorrect.");
                        return;
                    }
                    if (!UserVerificationUtil.AuthTokenValid(mappedUser, entry.AuthToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Auth token was expired or incorrect");
                        return;
                    }

                    UserToTextEntry forumPost = connection.GetForumPost(mappedUser.Company, entry.JobEntryId, entry.ForumPostId);
                    if(forumPost == null)
                    {
                        WriteBodylessResponse(ctx, 404, "Not Found");
                        return;
                    }
                    if(forumPost.UserId != entry.UserId)
                    {
                        WriteBodyResponse(ctx, 401, "Unauthorized", "Cannot delete the post of a different user");
                        return;
                    }
                    if(!connection.RemoveForumPost(mappedUser.Company, entry.JobEntryId, forumPost))
                    {
                        WriteBodyResponse(ctx, 500, "Internal Server Error", "Error occured while removing forum post: " + connection.LastException.Message);
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

        private bool ValidateGetRequest(CompanyForumApiGetRequest request)
        {
            if (request.LoginToken == null || request.LoginToken.Equals(""))
                return false;
            if (request.UserId <= 0)
                return false;
            if (request.JobEntryId <= 0)
                return false;
            return true;
        }

        private bool ValidateDeleteRequest(CompanyForumApiDeleteRequest req)
        {
            if (req.LoginToken == null || req.LoginToken.Equals(""))
                return false;
            if (req.AuthToken == null || req.AuthToken.Equals(""))
                return false;
            if (req.ForumPostId <= 0)
                return false;
            if (req.JobEntryId <= 0)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;

        }
    }
}
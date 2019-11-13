using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;
using System.IO;

namespace OldManInTheShopServer.Net.Api
{
    [DataContract]
    class UserCreationRequest
    {
        [DataMember]
        public string Email;

        [DataMember]
        public string SecurityQuestion;
        
        [DataMember]
        public string Password;
        
        [DataMember]
        public string SecurityAnswer;
    }

    [DataContract]
    class UserLoginRequest
    {
        [DataMember]
        public string Email;

        [DataMember]
        public string Password;
    }

    [DataContract]
    class UserCheckLoginStatusRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;
    }

    class UserApi : ApiDefinition
    {
#if RELEASE
        public UserApi(int portIn) : base("https://+:" + portIn + "/user")
#elif DEBUG
        public UserApi(int portIn) : base("http://+:"+portIn+"/user")
#endif
        {
            POST += HandlePostRequest;
            PUT += HandlePutRequest;
        }

        /// <summary>
        /// Request for creating a base mechanic user account. Documention is found in the Web API Enumeration file
        /// in the /RepairJob/Requirements tab, starting at row 1
        /// </summary>
        /// <param name="ctx">The HttpListenerContext to respond to</param>
        public void HandlePostRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "No Body", "Request lacked a body");
                    return;
                }
                UserCreationRequest req = JsonDataObjectUtil<UserCreationRequest>.ParseObject(ctx);
                if (req == null)
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Request was in the wrong format");
                    return;
                }
                if (!ValidateCreationResponse(req))
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Not all fields of the request were filled");
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
                    var users = connection.GetUsersWhere(" Email = \"" + req.Email + "\"");
                    if (users == null)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
                        return;
                    }
                    if (users.Count > 0)
                    {
                        WriteBodyResponse(ctx, 409, "User Conflict", "User with email already exists");
                        return;
                    }
                    res = connection.AddUser(req.Email, req.Password, req.SecurityQuestion, req.SecurityAnswer);
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected ServerError", connection.LastException.Message);
                        return;
                    }
                    WriteBodylessResponse(ctx, 200, "OK");
                }
            } catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private bool ValidateCreationResponse(UserCreationRequest req)
        {
            if (req.Email == null || req.Email.Equals(""))
                return false;
            if (req.Password == null || req.Password.Equals(""))
                return false;
            if (req.SecurityAnswer == null || req.SecurityAnswer.Equals(""))
                return false;
            return !(req.SecurityQuestion == null || req.SecurityQuestion.Equals(""));
        }

        /// <summary>
        /// Request for a user to log in using their email and password. Documention is found in the Web API Enumeration file
        /// in the /RepairJob/Requirements tab, starting at row 21
        /// </summary>
        /// <param name="ctx">The HttpListenerContext to respond to</param>
        public void HandlePutRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "No Body", "Request lacked a body");
                    return;
                }
                string reqString = new StreamReader(ctx.Request.InputStream).ReadToEnd();
                UserLoginRequest req = JsonDataObjectUtil<UserLoginRequest>.ParseObject(reqString);
                if (req == null)
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Request was in the wrong format");
                    return;
                }
                if (!ValidateLoginRequest(req))
                {
                    UserCheckLoginStatusRequest req2 = JsonDataObjectUtil<UserCheckLoginStatusRequest>.ParseObject(reqString);
                    if (req2 != null && ValidateCheckLoginRequest(req2))
                    {
                        HandleCheckLoginRequest(ctx, req2);
                        return;
                    }
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Not all fields of the request were filled");
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
                    var users = connection.GetUsersWhere(" Email = \"" + req.Email + "\"");
                    if (users.Count == 0)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on the server");
                        return;
                    }
                    if (!UserVerificationUtil.VerifyLogin(users[0], req.Email, req.Password))
                    {
                        WriteBodyResponse(ctx, 401, "Unauthorized", "Email or password was incorrect");
                        return;
                    }
                    OverallUser loggedInUser = users[0];
                    LoggedTokens tokens = ExtractLoggedTokens(loggedInUser);
                    GenerateNewLoginToken(tokens);
                    if (!connection.UpdateUsersLoginToken(loggedInUser, tokens))
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected Server Error", "Failed to write login token to database");
                        return;
                    }
                    JsonDictionaryStringConstructor retConstructor = new JsonDictionaryStringConstructor();
                    retConstructor.SetMapping("token", tokens.BaseLoggedInToken);
                    retConstructor.SetMapping("userId", loggedInUser.UserId);
                    retConstructor.SetMapping("accessLevel", loggedInUser.AccessLevel);
                    WriteBodyResponse(ctx, 200, "OK", retConstructor.ToString(), "application/json");
                }
            } catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private bool ValidateLoginRequest(UserLoginRequest req)
        {
            return !(req.Email == null || req.Email.Equals("") || req.Password == null || req.Password.Equals(""));
        }

        private LoggedTokens ExtractLoggedTokens(OverallUser userIn)
        {
            string loggedTokensJson = userIn.LoggedTokens;
            loggedTokensJson = loggedTokensJson.Replace("\\\"", "\"");
            byte[] tokens = Encoding.UTF8.GetBytes(loggedTokensJson);
            MemoryStream stream = new MemoryStream(tokens);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LoggedTokens));
            LoggedTokens ret = serializer.ReadObject(stream) as LoggedTokens;
            return ret;
        }

        private void GenerateNewLoginToken(LoggedTokens tokens)
        {
            Random rand = new Random();
            byte[] loginToken = new byte[64]; 
            rand.NextBytes(loginToken);
            tokens.BaseLoggedInToken = MysqlDataConvertingUtil.ConvertToHexString(loginToken);
            DateTime now = DateTime.UtcNow;
            now = now.AddHours(3);
            tokens.BaseLoggedInTokenExpiration = now.ToString();
        }

        private void HandleCheckLoginRequest(HttpListenerContext ctx, UserCheckLoginStatusRequest req)
        {
            try
            {
                MySqlDataManipulator connection = new MySqlDataManipulator();
                using (connection)
                {
                    bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected ServerError", "Connection to database failed");
                        return;
                    }
                    var user = connection.GetUserById(req.UserId);
                    if(user == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User with the specified id was not found on the server");
                        return;
                    }
                    if(!UserVerificationUtil.LoginTokenValid(user, req.LoginToken))
                    {
                        WriteBodylessResponse(ctx, 401, "Unauthorized");
                        return;
                    }
                    WriteBodylessResponse(ctx, 200, "OK");
                }
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private bool ValidateCheckLoginRequest(UserCheckLoginStatusRequest req)
        {
            if (req.LoginToken == null)
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }
    }
}

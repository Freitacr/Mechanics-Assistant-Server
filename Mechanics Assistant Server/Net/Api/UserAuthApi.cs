using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Net.Api
{
    [DataContract]
    class SecurityQuestionRequest
    {
        [DataMember]
        public string LoginToken = default;
        [DataMember]
        public int UserId = default;
    }

    [DataContract]
    class AuthenticationRequest
    {
        [DataMember]
        public string LoginToken = default;
        [DataMember]
        public int UserId = default;
        [DataMember]
        public string SecurityQuestion = default;
        [DataMember]
        public string SecurityAnswer = default;
    }

    [DataContract]
    class AuthenticationCheckRequest
    {
        [DataMember]
        public int UserId = default;

        [DataMember]
        public string LoginToken = default;

        [DataMember]
        public string AuthToken = default;
    }

    class UserAuthApi : ApiDefinition
    {
#if RELEASE
        public UserAuthApi(int portIn) : base("https://+:" + portIn + "/user/auth")
#elif DEBUG
        public UserAuthApi(int portIn) : base("http://+:"+portIn+"/user/auth")
#endif
        {
            POST += HandlePostRequest;
            PUT += HandlePutRequest;
        }

        /// <summary>
        /// Request for retrieving a user's security question. Documention is found in the Web API Enumeration file
        /// in the /User/Auth tab, starting at row 1
        /// </summary>
        /// <param name="ctx">The HttpListenerContext to respond to</param>
        private void HandlePostRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "No Body", "Request lacked a body");
                    return;
                }
                SecurityQuestionRequest req = JsonDataObjectUtil<SecurityQuestionRequest>.ParseObject(ctx);
                if (req == null)
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Request was in the wrong format");
                    return;
                }
                if (!ValidateSecurityQuestionRequest(req))
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
                    var user = connection.GetUserById(req.UserId);
                    if (user == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(user, req.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Unauthorized", "Login Token was incorrect or expired");
                        return;
                    }
                    WriteBodyResponse(ctx, 200, "OK", user.SecurityQuestion);
                }
            }
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
            }
            catch (Exception)
            {
                WriteBodylessResponse(ctx, 500, "Internal Server Error");
            }
        }

        /// <summary>
        /// Request for a user to receive authentication to make edits to content they have access to. Documention is found in the Web API Enumeration file
        /// in the /RepairJob/Requirements tab, starting at row 21
        /// </summary>
        /// <param name="ctx">The HttpListenerContext to respond to</param>
        private void HandlePutRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "No Body", "Request lacked a body");
                    return;
                }
                string reqString = new StreamReader(ctx.Request.InputStream).ReadToEnd();
                AuthenticationRequest req = JsonDataObjectUtil<AuthenticationRequest>.ParseObject(reqString);
                if (req == null)
                {
                    WriteBodyResponse(ctx, 400, "Incorrect Format", "Request was in the wrong format");
                    return;
                }
                if (!ValidateAuthenticationRequest(req))
                {
                    AuthenticationCheckRequest req2 = JsonDataObjectUtil<AuthenticationCheckRequest>.ParseObject(reqString);
                    if(req2 != null && ValidateAuthCheckRequest(req2))
                    {
                        HandleAuthCheckRequest(ctx, req2);
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
                    var user = connection.GetUserById(req.UserId);
                    if (user == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(user, req.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Unauthorized", "Login Token is incorrect or expired");
                        return;
                    }
                    if (!UserVerificationUtil.VerifyAuthentication(user, req.SecurityQuestion, req.SecurityAnswer))
                    {
                        WriteBodyResponse(ctx, 401, "Unauthorized", "Security Answer was incorrect");
                        return;
                    }
                    LoginStatusTokens tokens = UserVerificationUtil.ExtractLoginTokens(user);
                    UserVerificationUtil.GenerateNewAuthToken(tokens);
                    if (!connection.UpdateUsersLoginToken(user, tokens))
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected Server Error", "Failed to write login token to database");
                        return;
                    }
                    JsonDictionaryStringConstructor retConstructor = new JsonDictionaryStringConstructor();
                    retConstructor.SetMapping("token", tokens.AuthToken);
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

        private bool ValidateSecurityQuestionRequest(SecurityQuestionRequest req)
        {
            if (req == null)
                return false;
            if (req.LoginToken == null || req.LoginToken.Equals("") || req.LoginToken.Equals("x''"))
                return false;
            return req.UserId > 0;
        }

        private bool ValidateAuthenticationRequest(AuthenticationRequest req)
        {
            if (req == null)
                return false;
            if (req.UserId <= 0)
                return false;
            if (req.SecurityQuestion == null || req.SecurityQuestion.Equals(""))
                return false;
            if (req.SecurityAnswer == null || req.SecurityAnswer.Equals(""))
                return false;
            return !(req.LoginToken == null || req.LoginToken.Equals("") || req.LoginToken.Equals("x''"));
        }

        private void HandleAuthCheckRequest(HttpListenerContext ctx, AuthenticationCheckRequest req)
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
                    if (user == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(user, req.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Unauthorized", "Login Token is incorrect or expired");
                        return;
                    }
                    if(!UserVerificationUtil.AuthTokenValid(user, req.AuthToken))
                    {
                        WriteBodylessResponse(ctx, 401, "Unauthorized");
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

        private bool ValidateAuthCheckRequest(AuthenticationCheckRequest req)
        {
            if (req.LoginToken == null || req.LoginToken.Equals("x''") || req.LoginToken.Equals(""))
                return false;
            if (req.AuthToken == null || req.AuthToken.Equals("x''") || req.AuthToken.Equals(""))
                return false;
            if (req.UserId <= 0)
                return false;
            return true;
        }
    }
}

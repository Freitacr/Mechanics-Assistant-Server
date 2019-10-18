using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using MechanicsAssistantServer.Data.MySql;
using MechanicsAssistantServer.Data.MySql.TableDataTypes;
using MechanicsAssistantServer.Util;

namespace MechanicsAssistantServer.Net.Api
{
    [DataContract]
    class SecurityQuestionRequest
    {
        [DataMember]
        public string LoginToken;
        [DataMember]
        public int UserId;
    }

    [DataContract]
    class AuthenticationRequest
    {
        [DataMember]
        public string LoginToken;
        [DataMember]
        public int UserId;
        [DataMember]
        public string SecurityQuestion;
        [DataMember]
        public string SecurityAnswer;
    }

    class UserAuthApi : ApiDefinition
    {

        public UserAuthApi(int portIn) : base("https://+:" + portIn + "/user/auth")
        {
            POST += HandlePostRequest;
            PUT += HandlePutRequest;
        }

        private void HandlePostRequest(HttpListenerContext ctx)
        {
            if (!ctx.Request.HasEntityBody)
            {
                WriteBodyResponse(ctx, 400, "No Body", "Request lacked a body");
                return;
            }
            SecurityQuestionRequest req = ParseSecurityQuestionRequest(ctx);
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
            bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
            if (!res)
            {
                WriteBodyResponse(ctx, 500, "Unexpected ServerError", "Connection to database failed");
                return;
            }
            var user = connection.GetUserById(req.UserId);
            if(user == null)
            {
                WriteBodyResponse(ctx, 404, "Not Found", "User was not found on the server");
                return;
            }
            if(!UserVerificationUtil.LoginTokenValid(user, req.LoginToken))
            {
                WriteBodyResponse(ctx, 401, "Unauthorized", "Email or password was incorrect");
                return;
            }
            connection.Close();
            WriteBodyResponse(ctx, 200, "OK", user.SecurityQuestion);
        }

        private void HandlePutRequest(HttpListenerContext ctx)
        {
            if (!ctx.Request.HasEntityBody)
            {
                WriteBodyResponse(ctx, 400, "No Body", "Request lacked a body");
                return;
            }
            AuthenticationRequest req = ParseAuthenticationRequest(ctx);
            if (req == null)
            {
                WriteBodyResponse(ctx, 400, "Incorrect Format", "Request was in the wrong format");
                return;
            }
            if (!ValidateAuthenticationRequest(req))
            {
                WriteBodyResponse(ctx, 400, "Incorrect Format", "Not all fields of the request were filled");
                return;
            }
            MySqlDataManipulator connection = new MySqlDataManipulator();
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
                WriteBodyResponse(ctx, 401, "Unauthorized", "Email or password was incorrect");
                return;
            }
            if(!UserVerificationUtil.VerifyAuthentication(user, req.SecurityQuestion, req.SecurityAnswer))
            {
                WriteBodyResponse(ctx, 401, "Unauthorized", "Security Answer was incorrect");
                return;
            }
            LoggedTokens tokens = ExtractLoggedTokens(user);
            GenerateNewAuthToken(tokens);
            if (!connection.UpdateUsersLoginToken(user, tokens))
            {
                WriteBodyResponse(ctx, 500, "Unexpected Server Error", "Failed to write login token to database");
                return;
            }
            WriteBodyResponse(ctx, 200, "OK", tokens.AuthLoggedInToken);
            connection.Close();
        }

        private SecurityQuestionRequest ParseSecurityQuestionRequest(HttpListenerContext ctx)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SecurityQuestionRequest));
            SecurityQuestionRequest req;
            try
            {
                req = (SecurityQuestionRequest)serializer.ReadObject(ctx.Request.InputStream);
            }
            catch (SerializationException)
            {
                return null;
            }
            return req;
        }

        private bool ValidateSecurityQuestionRequest(SecurityQuestionRequest req)
        {
            if (req == null)
                return false;
            return !(req.LoginToken.Equals("") || req.LoginToken.Equals("0x"));
        }

        private AuthenticationRequest ParseAuthenticationRequest(HttpListenerContext ctx)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AuthenticationRequest));
            AuthenticationRequest req;
            try
            {
                req = (AuthenticationRequest)serializer.ReadObject(ctx.Request.InputStream);
            }
            catch (SerializationException)
            {
                return null;
            }
            return req;
        }

        private bool ValidateAuthenticationRequest(AuthenticationRequest req)
        {
            if (req == null)
                return false;
            if (req.SecurityQuestion.Equals(""))
                return false;
            if (req.SecurityAnswer.Equals(""))
                return false;
            return !(req.LoginToken.Equals("") || req.LoginToken.Equals("0x"));
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

        private void GenerateNewAuthToken(LoggedTokens tokens)
        {
            Random rand = new Random();
            byte[] loginToken = new byte[64];
            rand.NextBytes(loginToken);
            tokens.AuthLoggedInToken = MysqlDataConvertingUtil.ConvertToHexString(loginToken);
            DateTime now = DateTime.UtcNow;
            now = now.AddHours(.5);
            tokens.AuthLoggedInTokenExpiration = now.ToString();
        }
    }
}

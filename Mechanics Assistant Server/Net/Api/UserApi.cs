﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using MechanicsAssistantServer.Data.MySql.TableDataTypes;
using MechanicsAssistantServer.Data.MySql;
using MechanicsAssistantServer.Util;
using System.IO;

namespace MechanicsAssistantServer.Net.Api
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

    class UserApi : ApiDefinition
    {

        public UserApi(int portIn) : base("https://+:" + portIn + "/user")
        {
            POST += HandlePostRequest;
            PUT += HandlePutRequest;
        }

        public void HandlePostRequest(HttpListenerContext ctx)
        {
            if(!ctx.Request.HasEntityBody)
            {
                WriteErrorResponse(ctx, 400, "No Body", "Request lacked a body");
                return;
            }
            UserCreationRequest req = ParseRequest(ctx);
            if(req == null)
            {
                WriteErrorResponse(ctx, 400, "Incorrect Format", "Request was in the wrong format");
                return;
            }
            if(!ValidateCreationResponse(req))
            {
                WriteErrorResponse(ctx, 400, "Incorrect Format", "Not all fields of the request were filled");
                return;
            }
            MySqlDataManipulator connection = new MySqlDataManipulator();
            bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
            if (!res)
            {
                WriteErrorResponse(ctx, 500, "Unexpected ServerError", "Connection to database failed");
                return;
            }
            var users = connection.GetUsersWhere(" Email = \"" + req.Email + "\"");
            if(users == null)
            {
                WriteErrorResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
                return;
            }
            if(users.Count > 0)
            {
                WriteErrorResponse(ctx, 409, "User Conflict", "User with email already exists");
                return;
            }
            res = connection.AddUser(req.Email, req.Password, req.SecurityQuestion, req.SecurityAnswer);
            if(!res)
            {
                WriteErrorResponse(ctx, 500, "Unexpected ServerError", connection.LastException.Message);
                return;
            }
            WriteBodylessResponse(ctx, 200, "OK");
            connection.Close();
        }

        private UserCreationRequest ParseRequest(HttpListenerContext ctx)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(UserCreationRequest));
            UserCreationRequest req;
            try
            {
                req = (UserCreationRequest) serializer.ReadObject(ctx.Request.InputStream);
            } catch(SerializationException)
            {
                return null;
            }
            return req;
        }

        private bool ValidateCreationResponse(UserCreationRequest req)
        {
            if (req.Email.Equals(""))
                return false;
            if (req.Password.Equals(""))
                return false;
            if (req.SecurityAnswer.Equals(""))
                return false;
            return !req.SecurityQuestion.Equals("");
        }

        public void HandlePutRequest(HttpListenerContext ctx)
        {
            if (!ctx.Request.HasEntityBody)
            {
                WriteErrorResponse(ctx, 400, "No Body", "Request lacked a body");
                return;
            }
            UserLoginRequest req = ParseLoginRequest(ctx);
            if (req == null)
            {
                WriteErrorResponse(ctx, 400, "Incorrect Format", "Request was in the wrong format");
                return;
            }
            if (!ValidateLoginRequest(req))
            {
                WriteErrorResponse(ctx, 400, "Incorrect Format", "Not all fields of the request were filled");
                return;
            }
            MySqlDataManipulator connection = new MySqlDataManipulator();
            bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
            if (!res)
            {
                WriteErrorResponse(ctx, 500, "Unexpected ServerError", "Connection to database failed");
                return;
            }
            var users = connection.GetUsersWhere(" Email = \"" + req.Email + "\"");
            if(users.Count == 0)
            {
                WriteErrorResponse(ctx, 404, "Not Found", "User was not found on the server");
                return;
            }
            if(!UserVerificationUtil.VerifyLogin(users[0], req.Email, req.Password))
            {
                WriteErrorResponse(ctx, 401, "Unauthorized", "Email or password was incorrect");
                return;
            }
            OverallUser loggedInUser = users[0];
            LoggedTokens tokens = ExtractLoggedTokens(loggedInUser);
            GenerateNewLoginToken(tokens);
            if(!connection.UpdateUsersLoginToken(loggedInUser, tokens))
            {
                WriteErrorResponse(ctx, 500, "Unexpected Server Error", "Failed to write login token to database");
                return;
            }
            WriteErrorResponse(ctx, 200, "OK", tokens.BaseLoggedInToken);
            connection.Close();
        }

        private UserLoginRequest ParseLoginRequest(HttpListenerContext ctx)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(UserLoginRequest));
            UserLoginRequest req;
            try
            {
                req = (UserLoginRequest)serializer.ReadObject(ctx.Request.InputStream);
            }
            catch (SerializationException)
            {
                return null;
            }
            return req;
        }

        private bool ValidateLoginRequest(UserLoginRequest req)
        {
            return !(req.Email.Equals("") || req.Password.Equals(""));
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
    }
}

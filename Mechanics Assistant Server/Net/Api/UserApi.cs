using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using MechanicsAssistantServer.Data.MySql.TableDataTypes;
using MechanicsAssistantServer.Data.MySql;

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
            var users = connection.GetUserWhere(" \"Email\" = \"" + req.Email + "\"");
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
    }
}

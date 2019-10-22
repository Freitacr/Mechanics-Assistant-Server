using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Net;
using MechanicsAssistantServer.Data.MySql.TableDataTypes;
using MechanicsAssistantServer.Data.MySql;
using MechanicsAssistantServer.Util;

namespace MechanicsAssistantServer.Net.Api
{
    [DataContract]
    class RepairJobApiFullRequest
    {
        [DataMember]
        public JobDataEntry ContainedEntry;

        [DataMember]
        public int UserId;

        /**
         * <summary>JSON String. Format provided in LoggedTokens</summary>
         */
        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;
    }

    class RepairJobApi : ApiDefinition
    {
#if RELEASE
        public RepairJobApi(int port) : base("https://+:" + port + "/repairjob")
#elif DEBUG
        public RepairJobApi(int port) : base("http://+:" + port + "/repairjob")
#endif
        {
            POST += HandlePostRequest;
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
                RepairJobApiFullRequest entry = JsonDataObjectUtil<RepairJobApiFullRequest>.ParseObject(ctx);
                if (!ValidateFullRequest(entry))
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
                    if(mappedUser==null)
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

                    //Now that we know the user is good, actually do the addition.
                    res = connection.AddDataEntry(mappedUser.Company, entry.ContainedEntry);
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
                        return;
                    }
                    WriteBodylessResponse(ctx, 200, "OK");
                }
            } catch(Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private bool ValidateJobDataEntry(JobDataEntry entryIn)
        {
            if (entryIn == null)
                return false;
            if (entryIn.JobId == "")
                return false;
            if (entryIn.Make == "")
                return false;
            if (entryIn.Model == "")
                return false;
            if (entryIn.Complaint == "")
                return false;
            if (entryIn.Problem == "")
                return false;
            return true;
        }

        private bool ValidateFullRequest(RepairJobApiFullRequest req)
        {
            if (!ValidateJobDataEntry(req.ContainedEntry))
                return false;
            if (req.LoginToken == "")
                return false;
            if (req.UserId == -1)
                return false;
            return true;
        }
    }
}

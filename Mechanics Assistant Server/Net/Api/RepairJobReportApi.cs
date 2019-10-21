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
    class RepairJobVoteRequest
    {
        [DataMember]
        public int UserId;

        [DataMember]
        public bool Upvote;

        [DataMember]
        public int RepairJobId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;
    }

    class RepairJobReportApi : ApiDefinition
    {

#if RELEASE
        public RepairJobReportApi(int port) : base("https://+:" + port + "/repairjob/report")
#elif DEBUG
        public RepairJobReportApi(int port) : base("http://+:" + port + "/repairjob/report")
#endif
        {
            PUT += HandlePutRequest;
        }

        private void HandlePutRequest(HttpListenerContext ctx) {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                RepairJobVoteRequest entry = JsonDataObjectUtil<RepairJobVoteRequest>.ParseObject(ctx);
                if (entry == null)
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
                    JobDataEntry repairEntry = connection.GetDataEntryById(mappedUser.Company, entry.RepairJobId, true);
                    if (repairEntry == null && connection.LastException == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "Referenced Repair Job Was Not Found");
                        return;
                    }
                    else if (repairEntry == null)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
                        return;
                    }
                    if (entry.Upvote == false)
                    {
                        if (!connection.UpdateValidationStatus(mappedUser.Company, repairEntry, true))
                        {
                            WriteBodyResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
                            return;
                        }
                    }
                    else
                    {
                        NotSupported(ctx);
                        return;
                    }
                    WriteBodylessResponse(ctx, 200, "OK");
                }
            } catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }
    }
}

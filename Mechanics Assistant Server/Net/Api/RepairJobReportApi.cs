using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Net;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Net.Api
{
    
    [DataContract]
    class RepairJobVoteRequest
    {
        [DataMember]
        public int UserId = default;

        [DataMember]
        public int Upvote = default;

        [DataMember]
        public int RepairJobId = default;

        [DataMember]
        public string LoginToken = default;

        [DataMember]
        public string AuthToken = default;
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

        /// <summary>
        /// Request for either negatively or positively reporting a repair job entry. Documention is found in the Web API Enumeration file
        /// in the /RepairJob/Report tab, starting at row 1
        /// </summary>
        /// <param name="ctx">The HttpListenerContext to respond to</param>
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
                    if(mappedUser == null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User Not Found");
                        return;
                    }
                    if (!ValidateVoteRequest(entry))
                    {
                        WriteBodyResponse(ctx, 400, "Invalid Format", "Request was in incorrect format");
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
                    RepairJobEntry repairEntry = connection.GetDataEntryById(mappedUser.Company, entry.RepairJobId, true);
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
                    if (entry.Upvote == 0)
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

        private bool ValidateVoteRequest(RepairJobVoteRequest req)
        {
            if (req.AuthToken == null || req.AuthToken.Equals("0x") || req.AuthToken.Equals(""))
                return false;
            if (req.LoginToken == null || req.LoginToken.Equals("0x") || req.LoginToken.Equals(""))
                return false;
            if (req.Upvote < 0 || req.Upvote > 1)
                return false;
            if (req.UserId < 1)
                return false;
            if (req.RepairJobId < 1)
                return false;
            return true;
        }
    }
}

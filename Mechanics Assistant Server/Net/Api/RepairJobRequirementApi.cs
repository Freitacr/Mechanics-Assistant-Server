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
    class RequirementsPostRequest
    {
        [DataMember]
        public string RequirementString;

        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;

        [DataMember]
        public int RepairJobId;
    }

    [DataContract]
    class RequirementsDownvoteRequest
    {
        [DataMember]
        public int RepairJobId;

        [DataMember]
        public int RequirementId;

        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;
    }

    [DataContract]
    class RequirementsEditRequest
    {
        [DataMember]
        public int RepairJobId;

        [DataMember]
        public int RequirementId;

        [DataMember]
        public int UserId;

        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;

        [DataMember]
        public string NewRequirementValue;
    }

    class RepairJobRequirementApi : ApiDefinition
    {

#if DEBUG
        public RepairJobRequirementApi(int portIn) : base("http://+:"+portIn+"/repairjob/requirements")
#elif RELEASE
        public RepairJobRequirementApi(int portIn) : base ("https://+:" + portIn+"/repairjob/requirements")
#endif
        {
            POST += HandlePostRequest;
            PATCH += HandlePatchRequest;
            DELETE += HandleDeleteRequest;
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

                RequirementsPostRequest entry = JsonDataObjectUtil<RequirementsPostRequest>.ParseObject(ctx);
                if (!ValidatePostRequest(entry))
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
                    JobDataEntry repairEntry = connection.GetDataEntryById(mappedUser.Company, entry.RepairJobId, false);
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
                    //User is authenticated, and the job entry exists.... time to edit the requirements
                    RequirementsEntry requirementsEntry = RequirementsEntry.ParseJsonString(repairEntry.Requirements);
                    if (requirementsEntry == null)
                    {
                        WriteBodylessResponse(ctx, 500, "Unexpected Server Error");
                        return;
                    }
                    requirementsEntry.Auxillary.Add(new AuxillaryRequirement() { Downvotes = 0, Requirement = entry.RequirementString, UserId = entry.UserId });
                    repairEntry.Requirements = requirementsEntry.GenerateJsonString();
                    if (!connection.UpdateDataEntryRequirements(mappedUser.Company, repairEntry, false))
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
                        return;
                    }
                    WriteBodylessResponse(ctx, 200, "OK");
                }
            } catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private void HandleDeleteRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }

                RequirementsDownvoteRequest entry = JsonDataObjectUtil<RequirementsDownvoteRequest>.ParseObject(ctx);
                if (!ValidateDownvoteRequest(entry))
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
                    JobDataEntry repairEntry = connection.GetDataEntryById(mappedUser.Company, entry.RepairJobId, false);
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
                    //User is authenticated, and the job entry exists.... time to edit the requirements
                    RequirementsEntry requirementsEntry = RequirementsEntry.ParseJsonString(repairEntry.Requirements);
                    if (requirementsEntry == null)
                    {
                        WriteBodylessResponse(ctx, 500, "Unexpected Server Error");
                        return;
                    }
                    if (entry.RequirementId >= requirementsEntry.Auxillary.Count)
                    {
                        WriteBodylessResponse(ctx, 404, "Could not find a requirement with id " + entry.RequirementId);
                    }
                    if (requirementsEntry.Auxillary[entry.RequirementId].UserId == mappedUser.UserId
                            || (mappedUser.AccessLevel & AccessLevelMasks.AdminMask) != 0)
                    {
                        requirementsEntry.Auxillary.RemoveAt(entry.RequirementId);
                    }
                    else
                    {
                        requirementsEntry.Auxillary[entry.RequirementId].Downvotes++;
                        //TODO: Once company settings are completed, search in the company's settings to see if the
                        //downvotes are high enough to warrent removal
                    }
                    repairEntry.Requirements = requirementsEntry.GenerateJsonString();
                    if (!connection.UpdateDataEntryRequirements(mappedUser.Company, repairEntry, false))
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
                        return;
                    }
                    WriteBodylessResponse(ctx, 200, "OK");
                }
            } catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }

        private void HandlePatchRequest(HttpListenerContext ctx)
        {
            try
            {
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }

                RequirementsEditRequest entry = JsonDataObjectUtil<RequirementsEditRequest>.ParseObject(ctx);
                if (!ValidateEditRequest(entry))
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
                    JobDataEntry repairEntry = connection.GetDataEntryById(mappedUser.Company, entry.RepairJobId, false);
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
                    //User is authenticated, and the job entry exists.... time to edit the requirements
                    RequirementsEntry requirementsEntry = RequirementsEntry.ParseJsonString(repairEntry.Requirements);
                    if (requirementsEntry == null)
                    {
                        WriteBodylessResponse(ctx, 500, "Unexpected Server Error");
                        return;
                    }
                    if (entry.RequirementId >= requirementsEntry.Auxillary.Count)
                    {
                        WriteBodylessResponse(ctx, 404, "Could not find a requirement with id " + entry.RequirementId);
                        return;
                    }
                    requirementsEntry.Auxillary[entry.RequirementId].Requirement = entry.NewRequirementValue;
                    repairEntry.Requirements = requirementsEntry.GenerateJsonString();
                    if (!connection.UpdateDataEntryRequirements(mappedUser.Company, repairEntry, false))
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

        private bool ValidatePostRequest(RequirementsPostRequest req)
        {
            if (req.RepairJobId < 1)
                return false;
            if (req.RequirementString.Equals(""))
                return false;
            if (req.UserId < 1)
                return false;
            return true;
        }

        private bool ValidateEditRequest(RequirementsEditRequest req)
        {
            if (req.RepairJobId < 1)
                return false;
            if (req.NewRequirementValue.Equals(""))
                return false;
            if (req.RequirementId < 0)
                return false;
            if (req.UserId < 1)
                return false;
            return true;
        }

        private bool ValidateDownvoteRequest(RequirementsDownvoteRequest req)
        {
            if (req.RepairJobId < 1)
                return false;
            if (req.RequirementId < 0)
                return false;
            if (req.UserId < 1)
                return false;
            return true;
        }
    }
}

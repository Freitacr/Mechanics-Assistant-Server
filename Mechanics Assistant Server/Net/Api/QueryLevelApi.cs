using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using OldManInTheShopServer.Models;
using OldManInTheShopServer.Data;
using System;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Net.Api
{
    /**<summary>Api definition for all query requests</summary>*/
    class QueryLevelApi : ApiDefinition
    {

        private QueryProcessor QueryProcessor;
#if RELEASE
        public QueryLevelApi(int port, QueryProcessor processorIn) : base("https://+:" + port + "/query")
#elif DEBUG
        public QueryLevelApi(int port, QueryProcessor processorIn) : base("http://+:" + port + "/query")
#endif
        {
            QueryProcessor = processorIn;
            PUT += HandlePutRequestHtml;
        }

        private MechanicQuery ReadMechanicQuery (Stream streamIn)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(MechanicQuery));
            return serializer.ReadObject(streamIn) as MechanicQuery;
        }

        /**<summary>Responds to the put request made by a client</summary>
         * <example>If the user inquires with a bad request in the body of their http request, they will receive an http code 400 response</example>
         * <example>If the user inquires with a valid request, then they are sent back html code corresponding to a table containing the possible 
         * problems for the query.</example>
         */
        public void HandlePutRequestHtml(HttpListenerContext ctxIn)
        {
            try
            {
                if (!new List<string>(ctxIn.Request.AcceptTypes).Contains("text/html"))
                    return;
                if (!ctxIn.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctxIn, 400, "No Body", "Request contained no body");
                    return;
                }
                MechanicQuery query = ReadMechanicQuery(ctxIn.Request.InputStream);
                query.Make = query.Make.ToLower();
                query.Model = query.Model.ToLower();
                List<string> possibleProblems = QueryProcessor.ProcessQuery(query);
                StringBuilder tableBuilder = new StringBuilder();
                tableBuilder.Append("<tr><th>Make</th><th>Model</th><th>Year</th><th>Problem</th><th>Similarity</th></tr>");
                foreach (string possibleProblem in possibleProblems)
                {
                    tableBuilder.Append("<tr>");
                    tableBuilder.Append("<td></td>");
                    tableBuilder.Append("<td></td>");
                    tableBuilder.Append("<td></td>");
                    tableBuilder.Append("<td>" + possibleProblem + "</td>");
                    tableBuilder.Append("<td>0.0</td>");
                    tableBuilder.AppendLine("</tr>");
                }
                string tableString = tableBuilder.ToString();
                byte[] problemResp = Encoding.UTF8.GetBytes(tableString);
                WriteBodyResponse(ctxIn, 200, "OK", tableString, "text/html");
            }
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctxIn, 500, "Internal Server Error", "Error occurred while processing request: " + e.Message);
            }
        }

        /**<summary>Responds to the put request made by a client</summary>
         * <example>If the user inquires with a bad request in the body of their http request, they will receive an http code 400 response</example>
         * <example>If the user inquires with a valid request, then they are sent back a json list containing the possible 
         * problems for the query.</example>
         */
        public void HandlePutRequestJson(HttpListenerContext ctxIn)
        {
            try
            {
                if (!new List<string>(ctxIn.Request.AcceptTypes).Contains("application/json"))
                    return;
                if (!ctxIn.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctxIn, 400, "Bad Request", "No Body");
                    return;
                }
                MechanicQuery query = ReadMechanicQuery(ctxIn.Request.InputStream);
                query.Make = query.Make.ToLower();
                query.Model = query.Model.ToLower();
                List<string> possibleProblems = QueryProcessor.ProcessQuery(query);
                string problems = JsonDataObjectUtil<List<string>>.ConvertObject(possibleProblems);
                WriteBodyResponse(ctxIn, 200, "OK", problems, "application/json");
            }
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctxIn, 500, "Internal Server Error", e.Message);
            }
        }
        
        /**<summary>Responds to the post request made by a client</summary>
         * <example>If the user inquires with a bad request in the body of their http request, they will receive an http code 400 response</example>
         * <example>If the user inquires with a valid request, then the server attempts to add the query sent in to the active data store.</example>
         */
        public void HandlePostRequest(HttpListenerContext ctxIn)
        {
            try
            {
                if (!ctxIn.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctxIn, 400, "Bad Request", "No Body");
                    return;
                }
                MechanicQuery query = ReadMechanicQuery(ctxIn.Request.InputStream);
                if (!IsQueryViable(query))
                {
                    //Query was missing one or more fields
                    WriteBodyResponse(ctxIn, 400, "Bad Request", "Query was missing one or more fields");
                    return;
                }

                if (!QueryProcessor.AddData(query))
                {
                    //Error occurred. Server side.
                    WriteBodylessResponse(ctxIn, 500, "Internal Server Error");
                    return;
                }
                WriteBodylessResponse(ctxIn, 200, "OK");
            }
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctxIn, 500, "Internal Server Error", e.Message);
            }
        }

        private bool IsQueryViable(MechanicQuery queryIn)
        {
            bool ret = queryIn.Make == null || queryIn.Make.Length == 0;
            ret |= queryIn.Model == null || queryIn.Model.Length == 0;
            ret |= queryIn.Complaint == null || queryIn.Complaint.Length == 0;
            ret |= queryIn.Problem == null || queryIn.Problem.Length == 0;
            return !ret;
        }
    }
}

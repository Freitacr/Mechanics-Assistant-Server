using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using OldManinTheShopServer.Models;
using OldManinTheShopServer.Data;

namespace OldManinTheShopServer.Net.Api
{
    /**<summary>Api definition for all query requests</summary>*/
    class QueryLevelApi : ApiDefinition
    {

        private QueryProcessor QueryProcessor;
        
        public QueryLevelApi(int port, QueryProcessor processorIn) : base("https://+:" + port + "/query")
        {
            QueryProcessor = processorIn;
            PUT += HandlePutRequestJson;
            PUT += HandlePutRequestHtml;
            POST += HandlePostRequest;
            OPTIONS += HandleOptionRequest;
            //AddAction("put", HandlePutRequest);
            //AddAction("post", HandlePostRequest);
        }

        private MechanicQuery ReadMechanicQuery (Stream streamIn)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(MechanicQuery));
            return serializer.ReadObject(streamIn) as MechanicQuery;
        }

        public void HandleOptionRequest(HttpListenerContext ctxIn)
        {
            ctxIn.Response.StatusCode = 200;
            ctxIn.Response.AddHeader("Access-Control-Allow-Methods", "PUT,POST");
            ctxIn.Response.AddHeader("Access-Control-Allow-Origin", "*");
            ctxIn.Response.AddHeader("Access-Control-Allow-Headers", "*");
            ctxIn.Response.Close();
        }

        /**<summary>Responds to the put request made by a client</summary>
         * <example>If the user inquires with a bad request in the body of their http request, they will receive an http code 400 response</example>
         * <example>If the user inquires with a valid request, then they are sent back html code corresponding to a table containing the possible 
         * problems for the query.</example>
         */
        public void HandlePutRequestHtml(HttpListenerContext ctxIn)
        {
            if (!new List<string>(ctxIn.Request.AcceptTypes).Contains("text/html"))
                return;
            if (!ctxIn.Request.HasEntityBody)
            {
                ctxIn.Response.StatusCode = 400;
                ctxIn.Response.StatusDescription = "Bad Request";
                string body = "No Body";
                ctxIn.Response.ContentType = "text/plain";
                byte[] resp = Encoding.UTF32.GetBytes(body);
                ctxIn.Response.ContentLength64 = resp.LongLength;
                ctxIn.Response.OutputStream.Write(resp, 0, resp.Length);
                ctxIn.Response.OutputStream.Close();
                return;
            }
            MechanicQuery query = ReadMechanicQuery(ctxIn.Request.InputStream);
            query.Make = query.Make.ToLower();
            query.Model = query.Model.ToLower();
            List<string> possibleProblems = QueryProcessor.ProcessQuery(query);
            StringBuilder tableBuilder = new StringBuilder();
            tableBuilder.Append("<tr><th>Make</th><th>Model</th><th>Year</th><th>Problem</th><th>Similarity</th></tr>");
            foreach(string possibleProblem in possibleProblems)
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
            ctxIn.Response.StatusCode = 200;
            ctxIn.Response.AddHeader("Access-Control-Allow-Origin", "*");
            ctxIn.Response.AddHeader("Access-Control-Allow-Headers", "*");
            ctxIn.Response.StatusDescription = "OK";
            ctxIn.Response.ContentType = "text/html";
            ctxIn.Response.ContentLength64 = problemResp.LongLength;
            ctxIn.Response.OutputStream.Write(problemResp, 0, problemResp.Length);
            ctxIn.Response.OutputStream.Close();

        }

        /**<summary>Responds to the put request made by a client</summary>
         * <example>If the user inquires with a bad request in the body of their http request, they will receive an http code 400 response</example>
         * <example>If the user inquires with a valid request, then they are sent back a json list containing the possible 
         * problems for the query.</example>
         */
        public void HandlePutRequestJson(HttpListenerContext ctxIn)
        {
            if (!new List<string>(ctxIn.Request.AcceptTypes).Contains("application/json"))
                return;
            if(!ctxIn.Request.HasEntityBody)
            {
                ctxIn.Response.StatusCode = 400;
                ctxIn.Response.StatusDescription = "Bad Request";
                string body = "No Body";
                ctxIn.Response.ContentType = "text/plain";
                byte[] resp = Encoding.UTF32.GetBytes(body);
                ctxIn.Response.ContentLength64 = resp.LongLength;
                ctxIn.Response.OutputStream.Write(resp, 0, resp.Length);
                ctxIn.Response.OutputStream.Close();
                return;
            }
            MechanicQuery query = ReadMechanicQuery(ctxIn.Request.InputStream);
            query.Make = query.Make.ToLower();
            query.Model = query.Model.ToLower();
            List<string> possibleProblems = QueryProcessor.ProcessQuery(query);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<string>));
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, possibleProblems);
            byte[] problemResp = stream.ToArray();
            ctxIn.Response.StatusCode = 200;
            ctxIn.Response.StatusDescription = "OK";
            ctxIn.Response.ContentType = "application/json";
            ctxIn.Response.ContentLength64 = problemResp.LongLength;
            ctxIn.Response.OutputStream.Write(problemResp, 0, problemResp.Length);
            ctxIn.Response.OutputStream.Close();
        }
        
        /**<summary>Responds to the post request made by a client</summary>
         * <example>If the user inquires with a bad request in the body of their http request, they will receive an http code 400 response</example>
         * <example>If the user inquires with a valid request, then the server attempts to add the query sent in to the active data store.</example>
         */
        public void HandlePostRequest(HttpListenerContext ctxIn)
        {
            if (!ctxIn.Request.HasEntityBody)
            {
                ctxIn.Response.StatusCode = 400;
                ctxIn.Response.StatusDescription = "Bad Request";
                string body = "No Body";
                ctxIn.Response.ContentType = "text/plain";
                byte[] resp = Encoding.UTF8.GetBytes(body);
                ctxIn.Response.ContentLength64 = resp.LongLength;
                ctxIn.Response.OutputStream.Write(resp, 0, resp.Length);
                ctxIn.Response.OutputStream.Close();
                return;
            }
            MechanicQuery query = ReadMechanicQuery(ctxIn.Request.InputStream);
            if(!IsQueryViable(query))
            {
                //Query was missing one or more fields
                ctxIn.Response.StatusCode = 400;
                ctxIn.Response.StatusDescription = "Bad Request";
                string body = "{\"Bad Query\": \"Query was missing one or more fields\"}";
                ctxIn.Response.ContentType = "application/json";
                byte[] resp = Encoding.UTF8.GetBytes(body);
                ctxIn.Response.ContentLength64 = resp.LongLength;
                ctxIn.Response.OutputStream.Write(resp, 0, resp.Length);
                ctxIn.Response.OutputStream.Close();
                return;
            }

            if(!QueryProcessor.AddData(query))
            {
                //Error occurred. Server side.
                ctxIn.Response.StatusCode = 500;
                ctxIn.Response.StatusDescription = "Internal Server Error";
                ctxIn.Response.OutputStream.Close();
                return;
            }
            ctxIn.Response.StatusCode = 200;
            ctxIn.Response.StatusDescription = "OK";
            ctxIn.Response.OutputStream.Close();
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

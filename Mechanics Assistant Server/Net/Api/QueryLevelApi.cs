using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using MechanicsAssistantServer.Models;
using MechanicsAssistantServer.Data;

namespace MechanicsAssistantServer.Net.Api
{
    class QueryLevelApi : ApiDefinition
    {

        private QueryProcessor QueryProcessor;

        public QueryLevelApi(int port, QueryProcessor processorIn) : base("http://localhost:" + port + "/query")
        {
            QueryProcessor = processorIn;
            AddAction("put", HandlePutRequest);
            AddAction("post", HandlePostRequest);
        }

        private MechanicQuery ReadMechanicQuery (Stream streamIn)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(MechanicQuery));
            return serializer.ReadObject(streamIn) as MechanicQuery;
        }

        public void HandlePutRequest(HttpListenerContext ctxIn)
        {
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

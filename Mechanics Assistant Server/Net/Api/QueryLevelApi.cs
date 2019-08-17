using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using MechanicsAssistantServer.Models;
using MechanicsAssistantServer.Util;

namespace MechanicsAssistantServer.Net.Api
{
    class QueryLevelApi : ApiDefinition
    {

        private QueryProcessor QueryProcessor;

        public QueryLevelApi(int port, QueryProcessor processorIn) : base("http://localhost:" + port + "/query")
        {
            QueryProcessor = processorIn;
            AddAction("put", HandlePutRequest);
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
    }
}

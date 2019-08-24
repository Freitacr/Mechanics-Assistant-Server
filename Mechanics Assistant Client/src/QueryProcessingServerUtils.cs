using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Diagnostics;
using System.Threading;
using System.Net;

namespace MechanicsAssistantClient
{
    [DataContract]
    public class Query
    {
        [DataMember(Name = "make")]
        public string Make;
        [DataMember(Name = "model")]
        public string Model;
        [DataMember(Name = "complaint")]
        public string Complaint;
        [DataMember(Name = "problem", IsRequired = false)]
        public string Problem;

        public Query()
        {

        }

        public Query(string make, string model, string complaint)
        {
            Make = make;
            Model = model;
            Complaint = complaint;
        }

        public Query(string make, string model, string complaint, string problem) : this(make, model, complaint)
        {
            Problem = problem;
        }
    }

    class NoBodyException : Exception
    {
        public NoBodyException() : base("There was no body associated with the response") { }

    }

    public class QueryProcessingServerUtils
    {
        public static Process QueryServer;

        public static void StartServer()
        {
            ProcessStartInfo info = new ProcessStartInfo("dotnet.exe", "server.dll");
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            QueryServer = Process.Start(info);
        }

        public static List<string> ProcessQuery(Query query)
        {
            WebRequest req = WebRequest.CreateHttp("http://localhost:16384/query");
            req.Method = "PUT";
            req.ContentType = "application/json";
            req.Credentials = CredentialCache.DefaultCredentials;
            MemoryStream queryBytes = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Query));
            serializer.WriteObject(queryBytes, query);
            byte[] serialized = queryBytes.ToArray();
            req.ContentLength = serialized.LongLength;
            req.GetRequestStream().Write(serialized, 0, serialized.Length);
            req.GetRequestStream().Close();
            WebResponse resp = req.GetResponse();
            if (resp.ContentLength <= 0)
                throw new NoBodyException();
            if (resp.ContentType != "application/json")
                throw new FormatException("Response came in, but was not in JSON format");
            DataContractJsonSerializer listSerializer = new DataContractJsonSerializer(typeof(List<string>));
            List<string> ret = listSerializer.ReadObject(resp.GetResponseStream()) as List<string>;
            resp.GetResponseStream().Close();
            queryBytes.Close();
            return ret;
        }

        public static bool AddProblemToServer(Query query)
        {
            HttpWebRequest req = WebRequest.CreateHttp("http://localhost:16384/query");
            req.Method = "POST";
            req.ContentType = "application/json";
            req.Credentials = CredentialCache.DefaultCredentials;
            MemoryStream queryBytes = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Query));
            serializer.WriteObject(queryBytes, query);
            byte[] serialized = queryBytes.ToArray();
            req.ContentLength = serialized.LongLength;
            req.GetRequestStream().Write(serialized, 0, serialized.Length);
            req.GetRequestStream().Close();
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            bool ret = resp.StatusCode == HttpStatusCode.OK;
            resp.Close();
            return ret;
        }

        public static void CloseServer()
        {
            WebRequest req = WebRequest.CreateHttp("http://localhost:16384");
            req.Method = "DELETE";
            req.Credentials = CredentialCache.DefaultCredentials;
            req.GetResponse();
            for (int i = 0; i < 5 && !QueryServer.HasExited; i++)
                Thread.Sleep(1000);
            if (!QueryServer.HasExited)
                QueryServer.Kill();
        }
    }
}

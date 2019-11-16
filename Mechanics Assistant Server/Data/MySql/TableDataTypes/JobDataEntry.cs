using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    [DataContract]
    class AuxillaryRequirement
    {
        [DataMember]
        public string Requirement;

        [DataMember]
        public int UserId;

        [DataMember]
        public int Downvotes;
    }

    [DataContract]
    class RequirementsEntry
    {
        [DataMember]
        public List<string> Safety { get; set; } = new List<string>();
        
        [DataMember]
        public List<int> Parts { get; set; } = new List<int>();

        [DataMember]
        public List<AuxillaryRequirement> Auxillary { get; set; } = new List<AuxillaryRequirement>();

        public string GenerateJsonString()
        {
            MemoryStream outStream = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RequirementsEntry));
            serializer.WriteObject(outStream, this);
            byte[] retBytes = outStream.ToArray();
            return Encoding.UTF8.GetString(retBytes);
        }

        public static RequirementsEntry ParseJsonString(string stringIn)
        {
            byte[] reqBytes = Encoding.UTF8.GetBytes(stringIn);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RequirementsEntry));
            return serializer.ReadObject(new MemoryStream(reqBytes)) as RequirementsEntry;
        }

        public static string GenerateEmptyJson()
        {
            JsonDictionaryStringConstructor constructor = new JsonDictionaryStringConstructor();
            constructor.SetMapping("Safety", new List<object>());
            constructor.SetMapping("Parts", new List<object>());
            constructor.SetMapping("Auxillary", new List<object>());
            return constructor.ToString();
        }
    }

    [DataContract]
    public class JobDataEntry : ISqlSerializable
    {
        public static TableDataManipulator<JobDataEntry> Manipulator = new TableDataManipulator<JobDataEntry>();

        [DataMember]
        public string JobId { get; set; }
        
        [DataMember]
        public string Make { get; set; }

        [DataMember]
        public string Model { get; set; }

        [DataMember]
        public string Complaint { get; set; }

        [DataMember]
        public string Problem { get; set; }

        [DataMember(IsRequired = false)]
        public string ComplaintGroups { get; set; } = "[]";

        [DataMember(IsRequired = false)]
        public string ProblemGroups { get; set; } = "[]";

        [DataMember(IsRequired = false)]
        public string Requirements { get; set; } = new RequirementsEntry().GenerateJsonString();

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public int Year { get; set; } = -1;

        public int Id { get; private set; }

        public JobDataEntry()
        {

        }

        public JobDataEntry(string jobId, string make, string model, string complaint, string problem, string complaintGroups, string problemGroups, string requirements, int year)
        {
            JobId = jobId;
            Make = make;
            Model = model;
            Complaint = complaint;
            Problem = problem;
            ComplaintGroups = complaintGroups;
            ProblemGroups = problemGroups;
            Requirements = requirements;
            Year = year;
        }

        public void FillDefaultValues()
        {
            if(Requirements == null)
                Requirements = new RequirementsEntry().GenerateJsonString();
            if (ProblemGroups == null)
                ProblemGroups = "[]";
            if (ComplaintGroups == null)
                ComplaintGroups = "[]";
            if (Year == 0)
                Year = -1;
        }

        public ISqlSerializable Copy()
        {
            return new JobDataEntry(JobId, Make, Model, Complaint, Problem, ComplaintGroups, ProblemGroups, Requirements, Year);
        }

        public void Deserialize(MySqlDataReader reader)
        {
            JobId = (string)reader["JobId"];
            Make = (string)reader["Make"];
            Model = (string)reader["Model"];
            Complaint = (string)reader["Complaint"];
            Problem = (string)reader["Problem"];
            ComplaintGroups = (string)reader["ComplaintGroupings"];
            ProblemGroups = (string)reader["ProblemGroupings"];
            Requirements = (string)reader["Requirements"];
            Year = (int)reader["Year"];
            Id = (int)reader["id"];
        }

        public string Serialize(string tableName)
        {
            return "insert into " + tableName + "(JobId, Make, Model, Complaint, Problem, ComplaintGroupings, ProblemGroupings, Requirements, Year) values (\"" +
                JobId + "\",\"" + Make + "\",\"" + Model + "\",\"" + Complaint + "\",\"" + Problem +
                "\",\"" + ComplaintGroups.Replace("\"", "\\\"") + "\",\"" + ProblemGroups.Replace("\"", "\\\"") + "\",\"" + Requirements.Replace("\"", "\\\"") + "\"," + Year + ")";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var other = obj as JobDataEntry;
            return Make.Equals(other.Make) && Model.Equals(other.Model) &&
                Complaint.Equals(other.Complaint) && Problem.Equals(other.Problem);
        }

        public override int GetHashCode()
        {
            return Make.GetHashCode() + Model.GetHashCode() * Complaint.GetHashCode();
        }
    }
}

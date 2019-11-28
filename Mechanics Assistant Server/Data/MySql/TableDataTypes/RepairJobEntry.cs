using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using OldManInTheShopServer.Util;
using OldManInTheShopServer.Attribute;

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
    public class RepairJobEntry : MySqlTableDataMember<RepairJobEntry>
    {
        public static TableDataManipulator<RepairJobEntry> Manipulator = new TableDataManipulator<RepairJobEntry>();

        [DataMember]
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string JobId;

        [DataMember]
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string Make;

        [DataMember]
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string Model;

        [DataMember]
        [SqlTableMember("varchar(512)", MySqlDataFormatString = "\"{0}\"")]
        public string Complaint;

        [DataMember]
        [SqlTableMember("varchar(512)", MySqlDataFormatString = "\"{0}\"")]
        public string Problem;

        [DataMember(IsRequired = false)]
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string ComplaintGroups = "[]";

        [DataMember(IsRequired = false)]
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string ProblemGroups= "[]";

        [DataMember(IsRequired = false)]
        [SqlTableMember("varchar(1024)", MySqlDataFormatString = "\"{0}\"")]
        public string Requirements = new RequirementsEntry().GenerateJsonString();

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        [SqlTableMember("int")]
        public int Year = -1;

        public RepairJobEntry()
        {

        }

        public RepairJobEntry(string jobId, string make, string model, string complaint, string problem, string complaintGroups, string problemGroups, string requirements, int year)
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

        public override ISqlSerializable Copy()
        {
            return new RepairJobEntry(JobId, Make, Model, Complaint, Problem, ComplaintGroups, ProblemGroups, Requirements, Year);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var other = obj as RepairJobEntry;
            return Make.Equals(other.Make) && Model.Equals(other.Model) &&
                Complaint.Equals(other.Complaint) && Problem.Equals(other.Problem);
        }

        public override int GetHashCode()
        {
            return Make.GetHashCode() + Model.GetHashCode() * Complaint.GetHashCode();
        }

        protected override void ApplyDefaults()
        {
            ComplaintGroups = "[]";
            ProblemGroups = "[]";
            Requirements = new RequirementsEntry().GenerateJsonString();
            Year = -1;
        }

        public override string ToString()
        {
            return JobId ?? "";
        }
    }
}

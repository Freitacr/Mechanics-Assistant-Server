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
    /// <summary>
    /// Class representing an auxillary requirement of a <see cref="RepairJobEntry"/> added by a Mechanic level user
    /// </summary>
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

    /// <summary>
    /// Class representing the requirements attached to a <see cref="RepairJobEntry"/>.
    /// These requirements include Safety, Parts, and Auxillary requirements
    /// </summary>
    [DataContract]
    class RequirementsEntry
    {
        /// <summary>
        /// List of safety requirements associated with the <see cref="RepairJobEntry"/>
        /// </summary>
        [DataMember]
        public List<string> Safety { get; set; } = new List<string>();
        
        /// <summary>
        /// List of Database Ids of the parts required for the <see cref="RepairJobEntry"/>
        /// </summary>
        [DataMember]
        public List<int> Parts { get; set; } = new List<int>();

        /// <summary>
        /// List of auxillary requirements associated with the <see cref="RepairJobEntry"/>
        /// </summary>
        [DataMember]
        public List<AuxillaryRequirement> Auxillary { get; set; } = new List<AuxillaryRequirement>();

        /// <summary>
        /// Converts the current object to a JSON string based on its DataContract
        /// </summary>
        /// <returns>A JSON formatted string containing all the requirements associated with the <see cref="RepairJobEntry"/></returns>
        public string GenerateJsonString()
        {
            MemoryStream outStream = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RequirementsEntry));
            serializer.WriteObject(outStream, this);
            byte[] retBytes = outStream.ToArray();
            return Encoding.UTF8.GetString(retBytes);
        }

        /// <summary>
        /// Parses the string passed in an attempt to construct a RequirementsEntry object from it
        /// </summary>
        /// <param name="stringIn">JSON formatted string that represents a RequirementsEntry object</param>
        /// <returns>The RequirementsEntry object that was stored in JSON format within the string passed in</returns>
        /// <remarks>This method will break if a non-JSON formatted string is passed in</remarks>
        public static RequirementsEntry ParseJsonString(string stringIn)
        {
            byte[] reqBytes = Encoding.UTF8.GetBytes(stringIn);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RequirementsEntry));
            return serializer.ReadObject(new MemoryStream(reqBytes)) as RequirementsEntry;
        }

        /// <summary>
        /// Generates a JSON string that matches the format of a RequirementsEntry object containing no requirements
        /// </summary>
        /// <returns>A JSON string that matches the format of a RequirementsEntry object containing no requirements</returns>
        public static string GenerateEmptyJson()
        {
            JsonDictionaryStringConstructor constructor = new JsonDictionaryStringConstructor();
            constructor.SetMapping("Safety", new List<object>());
            constructor.SetMapping("Parts", new List<object>());
            constructor.SetMapping("Auxillary", new List<object>());
            return constructor.ToString();
        }
    }

    /// <summary>
    /// Class that represents a Repair Job that a mechanic has uploaded to the server, and how it should be stored
    /// in the MySql database
    /// </summary>
    [DataContract]
    public class RepairJobEntry : MySqlTableDataMember<RepairJobEntry>
    {
        public static TableDataManipulator<RepairJobEntry> Manipulator = new TableDataManipulator<RepairJobEntry>();

        /// <summary>
        /// The shop assigned id of the Repair Job
        /// </summary>
        [DataMember]
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string JobId;

        /// <summary>
        /// The make of the machine the repair job was about
        /// </summary>
        [DataMember]
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string Make;

        /// <summary>
        /// The model of the machine the repair job was about
        /// </summary>
        [DataMember]
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string Model;

        /// <summary>
        /// The customer's complaint about the machine that lead to the repair job being ordered
        /// </summary>
        [DataMember]
        [SqlTableMember("varchar(512)", MySqlDataFormatString = "\"{0}\"")]
        public string Complaint;

        /// <summary>
        /// The problem with the machine that the mechanic found in the process of fulfilling the
        /// repair job
        /// </summary>
        [DataMember]
        [SqlTableMember("varchar(512)", MySqlDataFormatString = "\"{0}\"")]
        public string Problem;

        /// <summary>
        /// A JSON formatted list of ints representing the database ids of the complaint groups that
        /// match with this repair job's complaint
        /// </summary>
        [DataMember(IsRequired = false)]
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string ComplaintGroups = "[]";

        /// <summary>
        /// A JSON formatted list of ints representing the database ids of the problem groups that
        /// match with this repair job's listed problem. Deprecated and unused.
        /// </summary>
        [DataMember(IsRequired = false)]
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string ProblemGroups= "[]";

        /// <summary>
        /// JSON string representing a <see cref="RequirementsEntry"/> object containing the requirements
        /// for this repair job
        /// </summary>
        [DataMember(IsRequired = false)]
        [SqlTableMember("varchar(1024)", MySqlDataFormatString = "\"{0}\"")]
        public string Requirements = new RequirementsEntry().GenerateJsonString();

        /// <summary>
        /// The year of the machine this repair job is about, a value of -1 represents an unknown year
        /// </summary>
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
            if (JobId == null)
                JobId = "";
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

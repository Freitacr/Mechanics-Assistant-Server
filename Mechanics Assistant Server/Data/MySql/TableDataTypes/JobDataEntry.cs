using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Runtime.Serialization;

namespace MechanicsAssistantServer.Data.MySql.TableDataTypes
{
    [DataContract]
    class JobDataEntry : ISqlSerializable
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
        public string ComplaintGroups { get; set; } = "";

        [DataMember(IsRequired = false)]
        public string ProblemGroups { get; set; } = "";

        [DataMember(IsRequired = false)]
        public string Requirements { get; set; } = "";

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public int Year { get; set; } = -1;

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
        }

        public string Serialize(string tableName)
        {
            return "insert into " + tableName + "(JobId, Make, Model, Complaint, Problem, ComplaintGroupings, ProblemGroupings, Requirements, Year) values (\"" +
                JobId + "\",\"" + Make + "\",\"" + Model + "\",\"" + Complaint + "\",\"" + Problem +
                "\",\"" + ComplaintGroups + "\",\"" + ProblemGroups + "\",\"" + Requirements + "\"," + Year + ")";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var other = obj as JobDataEntry;
            return other.JobId.Equals(JobId) && Make.Equals(other.Make) && Model.Equals(other.Model) &&
                Complaint.Equals(other.Complaint) && Problem.Equals(other.Problem);
        }

        public override int GetHashCode()
        {
            return Make.GetHashCode() + Model.GetHashCode() * Complaint.GetHashCode();
        }
    }
}

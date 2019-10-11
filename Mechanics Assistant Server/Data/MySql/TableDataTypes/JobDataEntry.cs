﻿using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace MechanicsAssistantServer.Data.MySql.TableDataTypes
{
    class JobDataEntry : ISqlSerializable
    {
        public static TableDataManipulator<JobDataEntry> Manipulator = new TableDataManipulator<JobDataEntry>();

        public string JobId { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Complaint { get; set; }
        public string Problem { get; set; }
        public string Groups { get; set; }
        public string Requirements { get; set; }
        public int Year { get; set; }

        public JobDataEntry()
        {

        }

        public JobDataEntry(string jobId, string make, string model, string complaint, string problem, string groups, string requirements, int year)
        {
            JobId = jobId;
            Make = make;
            Model = model;
            Complaint = complaint;
            Problem = problem;
            Groups = groups;
            Requirements = requirements;
            Year = year;
        }

        public ISqlSerializable Copy()
        {
            return new JobDataEntry(JobId, Make, Model, Complaint, Problem, Groups, Requirements, Year);
        }

        public void Deserialize(MySqlDataReader reader)
        {
            JobId = (string)reader["JobId"];
            Make = (string)reader["Make"];
            Model = (string)reader["Model"];
            Complaint = (string)reader["Complaint"];
            Problem = (string)reader["Problem"];
            Groups = (string)reader["Groupings"];
            Requirements = (string)reader["Requirements"];
            Year = (int)reader["Year"];
        }

        public string Serialize(string tableName)
        {
            return "insert into " + tableName + "(JobId, Make, Model, Complaint, Problem, Groupings, Requirements, Year) values (\"" +
                JobId + "\",\"" + Make + "\",\"" + Model + "\",\"" + Complaint + "\",\"" + Problem +
                "\",\"" + Groups + "\",\"" + Requirements + "\"," + Year + ")";
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

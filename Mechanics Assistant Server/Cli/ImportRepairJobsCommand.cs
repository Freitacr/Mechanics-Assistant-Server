using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Cli
{
    class ImportRepairJobsCommand : CommandLineCommand
    {
        [KeyedArgument("-i", true, "repairjob")]
        public string Flag = default;

        [PositionalArgument(0)]
        public string FilePath = default;

        [PositionalArgument(1)]
        public int CompanyId = default;

        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            StreamReader fileReader = new StreamReader(FilePath);
            string jsonString = fileReader.ReadToEnd();
            fileReader.Close();
            List<JobDataEntry> loadedData = JsonDataObjectUtil<List<JobDataEntry>>.ParseObject(jsonString);
            foreach (JobDataEntry entry in loadedData)
            {
                entry.Make = entry.Make.ToLower();
                entry.Model = entry.Model.ToLower();
                entry.Complaint = entry.Complaint.ToLower();
                entry.Problem = entry.Problem.ToLower();
                if (!manipulator.AddDataEntry(CompanyId, entry, validated: true))
                    Console.WriteLine(JsonDataObjectUtil<JobDataEntry>.ConvertObject(entry) + " was not added to the database");
            }
        }
    }
}

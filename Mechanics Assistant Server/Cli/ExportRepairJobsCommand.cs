using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using System.IO;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Cli
{
    class ExportRepairJobsCommand : CommandLineCommand
    {
        [KeyedArgument("-e", true, "repairjob")]
        public string Flag;

        [PositionalArgument(0)]
        public string FilePath;

        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            FileSystemDataSource dataSource = new FileSystemDataSource();
            List<MechanicQuery> queries = dataSource.LoadMechanicQueries();
            List<JobDataEntry> toWrite = new List<JobDataEntry>();
            foreach(MechanicQuery query in queries)
            {
                JobDataEntry toAdd = new JobDataEntry()
                {
                    Make = query.Make,
                    Model = query.Model,
                    Complaint = query.Complaint,
                    Problem = query.Problem,
                    JobId = "Unknown",
                    Year = -1
                };
                toWrite.Add(toAdd);
            }

            StreamWriter fileWriter = new StreamWriter(FilePath); //This is a CLI for devs, so no worries if this goes wonky
            fileWriter.WriteLine('[');
            

            foreach(JobDataEntry entry in toWrite)
            {
                string entryJson = JsonDataObjectUtil<JobDataEntry>.ConvertObject(entry);
                fileWriter.WriteLine(entryJson);
            }
            fileWriter.WriteLine(']');
            fileWriter.Close();
        }
    }
}

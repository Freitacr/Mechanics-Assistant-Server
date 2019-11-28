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
    /// <summary>
    /// Exports all stored <see cref="MechanicQuery"/> objects from the default <see cref="FileSystemDataSource"/> to the specified filePath
    /// in the new <see cref="RepairJobEntry"/> format.
    /// </summary>
    /// <remarks>As the <see cref="FileSystemDataSource"/> and <see cref="MechanicQuery"/> classes are deprecated, this command
    /// is kept only for legacy purposes</remarks>
    class ExportRepairJobsCommand : CommandLineCommand
    {
        /// <summary>
        /// Flag used to differentiate this command from the other commands in this package
        /// </summary>
        [KeyedArgument("-e", true, "repairjob")]
        public string Flag = default;

        /// <summary>
        /// File path (relative or absolute) used to specify the file the <see cref="RepairJobEntry"/> objects should be written to
        /// </summary>
        [PositionalArgument(0)]
        public string FilePath = default;

        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            //Convert all MechanicQueries to RepairJobEntries
            FileSystemDataSource dataSource = new FileSystemDataSource();
            List<MechanicQuery> queries = dataSource.LoadMechanicQueries();
            List<RepairJobEntry> toWrite = new List<RepairJobEntry>();
            foreach(MechanicQuery query in queries)
            {
                RepairJobEntry toAdd = new RepairJobEntry()
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

            //Write all RepairJobEntries to the specified file
            StreamWriter fileWriter = new StreamWriter(FilePath); //This is a CLI for devs, so no worries if this goes wonky
            fileWriter.WriteLine('[');
            

            foreach(RepairJobEntry entry in toWrite)
            {
                string entryJson = JsonDataObjectUtil<RepairJobEntry>.ConvertObject(entry);
                fileWriter.WriteLine(entryJson);
            }
            fileWriter.WriteLine(']');
            fileWriter.Close();
        }
    }
}

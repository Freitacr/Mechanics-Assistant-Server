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
    /// <summary>
    /// Command used to import a file containing a list of <see cref="RepairJobEntry"/> objects in JSON format
    /// </summary>
    class ImportRepairJobsCommand : CommandLineCommand
    {
        /// <summary>
        /// Flag used to differentiate this command from the other commands in this package
        /// </summary>
        [KeyedArgument("-i", true, "repairjob")]
        public string Flag = default;

        /// <summary>
        /// File path of the file to import
        /// </summary>
        [PositionalArgument(0)]
        public string FilePath = default;

        /// <summary>
        /// Database id of the company to add the <see cref="RepairJobEntry"/> objects to. These objects
        /// will be stored in the validated data of the company specified
        /// </summary>
        [PositionalArgument(1)]
        public int CompanyId = default;

        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            StreamReader fileReader = new StreamReader(FilePath);
            string jsonString = fileReader.ReadToEnd();
            fileReader.Close();
            List<RepairJobEntry> loadedData = JsonDataObjectUtil<List<RepairJobEntry>>.ParseObject(jsonString);
            foreach (RepairJobEntry entry in loadedData)
            {
                entry.Make = entry.Make.ToLower();
                entry.Model = entry.Model.ToLower();
                entry.Complaint = entry.Complaint.ToLower();
                entry.Problem = entry.Problem.ToLower();
                if (!manipulator.AddDataEntry(CompanyId, entry, validated: true))
                    Console.WriteLine(JsonDataObjectUtil<RepairJobEntry>.ConvertObject(entry) + " was not added to the database");
            }
        }
    }
}

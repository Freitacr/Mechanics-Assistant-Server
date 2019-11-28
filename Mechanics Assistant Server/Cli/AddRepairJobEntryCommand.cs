using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Cli
{
    
    class AddRepairJobEntryCommand : CommandLineCommand
    {
        [KeyedArgument("-c", true, "repairjob")]
        public string Flag = default;

        [PositionalArgument(0)]
        public string Make = default;

        [PositionalArgument(1)]
        public string Model = default;

        [PositionalArgument(2)]
        public string Complaint = default;
        
        [PositionalArgument(3)]
        public string Problem = default;

        [PositionalArgument(4)]
        public string JobId = default;

        [PositionalArgument(5)]
        public int CompanyId = default;

        [PositionalArgument(6)]
        public bool IsValidated = default;

        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            JobDataEntry e = new JobDataEntry()
            {
                JobId = JobId, 
                Make = Make, 
                Model = Model, 
                Complaint = Complaint, 
                Problem = Problem
            };

            if (!manipulator.AddDataEntry(CompanyId, e, IsValidated)){
                Console.WriteLine("Failed to add data entry");
            }
            Console.WriteLine("Successfully added data entry");
        }
    }
}

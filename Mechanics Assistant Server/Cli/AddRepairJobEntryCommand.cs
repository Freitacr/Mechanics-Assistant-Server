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
        public string Flag;

        [PositionalArgument(0)]
        public string Make;

        [PositionalArgument(1)]
        public string Model;

        [PositionalArgument(2)]
        public string Complaint;
        
        [PositionalArgument(3)]
        public string Problem;

        [PositionalArgument(4)]
        public string JobId;

        [PositionalArgument(5)]
        public int CompanyId;

        [PositionalArgument(6)]
        public bool IsValidated;

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

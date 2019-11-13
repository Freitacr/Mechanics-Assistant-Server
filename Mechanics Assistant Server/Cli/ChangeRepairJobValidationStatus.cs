using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;

namespace OldManInTheShopServer.Cli
{
    class ChangeRepairJobValidationStatus : CommandLineCommand
    {
        [KeyedArgument("-u", true, "validation")]
        public string Flag;

        [PositionalArgument(0)]
        public int RepairEntryId;

        [PositionalArgument(1)]
        public int CompanyId;

        [PositionalArgument(2)]
        public bool CurrentlyValidated;

        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            int companyId = CompanyId;
            int repairEntryId = RepairEntryId;
            bool isValidated = CurrentlyValidated;
            JobDataEntry entry = manipulator.GetDataEntryById(companyId, repairEntryId, isValidated);
            if(!manipulator.UpdateValidationStatus(companyId, entry, isValidated))
            {
                Console.WriteLine("Failed to update validation status");
                return;
            }
            Console.WriteLine("Updated validation status for entry with id " + repairEntryId);
        }
    }
}

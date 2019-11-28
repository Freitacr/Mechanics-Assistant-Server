using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;

namespace OldManInTheShopServer.Cli
{
    /// <summary>
    /// <para><see cref="CommandLineCommand"/> used to change the validation status of a <see cref="RepairJobEntry"/>
    /// in the company's data tables</para>
    /// </summary>
    /// 
    /// <remarks>
    /// A change in the validation status means that the <see cref="RepairJobEntry"/> will change tables
    /// 
    /// So if a particular entry is currently in the non validated data table, it will be moved to the validated
    /// one, and have a forum page created for it, and vice versa if it is currently in the validated data table
    /// </remarks>
    class ChangeRepairJobValidationStatus : CommandLineCommand
    {
        /// <summary>
        /// Flag used to differentiate this command from other commands in this package
        /// </summary>
        [KeyedArgument("-u", true, "validation")]
        public string Flag = default;

        /// <summary>
        /// Database id of the <see cref="RepairJobEntry"/> to update the validation status of
        /// </summary>
        [PositionalArgument(0)]
        public int RepairEntryId = default;

        /// <summary>
        /// Database id of the company to look through the data of
        /// </summary>
        [PositionalArgument(1)]
        public int CompanyId = default;

        /// <summary>
        /// Flag specifying whether to look in the company's validated or non validated data table for
        /// the <see cref="RepairJobEntry"/> to update
        /// </summary>
        [PositionalArgument(2)]
        public bool CurrentlyValidated = default;

        /// <summary>
        /// Function to perform the update of the <see cref="RepairJobEntry"/>'s validation status
        /// </summary>
        /// <param name="manipulator"><see cref="MySqlDataManipulator"/> used to update the <see cref="RepairJobEntry"/>'s 
        /// validation status</param>
        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            RepairJobEntry entry = manipulator.GetDataEntryById(CompanyId, RepairEntryId, CurrentlyValidated);
            if(!manipulator.UpdateValidationStatus(CompanyId, entry, CurrentlyValidated))
            {
                Console.WriteLine("Failed to update validation status");
                return;
            }
            Console.WriteLine("Updated validation status for entry with id " + RepairEntryId);
        }
    }
}

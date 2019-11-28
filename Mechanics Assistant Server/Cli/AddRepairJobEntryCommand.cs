using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Cli
{
    
    /// <summary>
    /// <para><see cref="CommandLineCommand"/> used to add a <see cref="RepairJobEntry"/> to the database</para>
    /// </summary>
    class AddRepairJobEntryCommand : CommandLineCommand
    {
        /// <summary>
        /// <para>Flag used to differentiate this command from other CommandLineCommands</para>
        /// </summary>
        [KeyedArgument("-c", true, "repairjob")]
        public string Flag = default;

        /// <summary>
        /// <para>Make of the machine the <see cref="RepairJobEntry"/> is about</para>
        /// </summary>
        [PositionalArgument(0)]
        public string Make = default;

        /// <summary>
        /// <para>Model of the machine the <see cref="RepairJobEntry"/> is about</para>
        /// </summary>
        [PositionalArgument(1)]
        public string Model = default;

        /// <summary>
        /// <para>Customer's complaint about the machine the <see cref="RepairJobEntry"/> is about</para>
        /// </summary>
        [PositionalArgument(2)]
        public string Complaint = default;
        
        /// <summary>
        /// <para>The problem the mechanic found with the machine the <see cref="RepairJobEntry"/> is about</para>
        /// </summary>
        [PositionalArgument(3)]
        public string Problem = default;

        /// <summary>
        /// <para>The shop-assigned job id of the <see cref="RepairJobEntry"/></para>
        /// </summary>
        [PositionalArgument(4)]
        public string JobId = default;

        /// <summary>
        /// <para>Database id of the company to add the <see cref="RepairJobEntry"/> to</para>
        /// </summary>
        [PositionalArgument(5)]
        public int CompanyId = default;

        /// <summary>
        /// <para>Flag to sepcify whether the <see cref="RepairJobEntry"/> should be added to the company's validated data set 
        /// or non validated data set</para>
        /// </summary>
        [PositionalArgument(6)]
        public bool IsValidated = default;

        /// <summary>
        /// Constructs a <see cref="RepairJobEntry"/> from the fields of this object 
        /// and adds it to the database in the specified location
        /// </summary>
        /// <param name="manipulator"><see cref="MySqlDataManipulator"/> used to add the <see cref="RepairJobEntry"/> to the database</param>
        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            RepairJobEntry e = new RepairJobEntry()
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

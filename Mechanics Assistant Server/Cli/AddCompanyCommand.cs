using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;

namespace OldManInTheShopServer.Cli
{
    class AddCompanyCommand : CommandLineCommand
    {
        [KeyedArgument("-c", true, "company")]
        public string Flag;
        [PositionalArgument(0)]
        public string LegalName;

        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            if (!manipulator.AddCompany(LegalName))
            {
                Console.WriteLine("Failed to add company " + string.Join(" ", LegalName));
                Console.WriteLine("Failed because of error " + manipulator.LastException.Message);
                return;
            }
            Console.WriteLine("Successfully added company " + string.Join(" ", LegalName));
            
        }
    }
}

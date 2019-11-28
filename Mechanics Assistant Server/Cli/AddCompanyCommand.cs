using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;

namespace OldManInTheShopServer.Cli
{
    /// <summary>
    /// <para><see cref="CommandLineCommand"/> to add a company to the database</para>
    /// <para>An example of this command, assuming the executable's name was foo is as follows:</para>
    /// <para><code>foo -c company "Test Company"</code></para>
    /// <para>This would add the company Test Company to the database, along with all the tables needed for
    /// company functionality</para>
    /// </summary>
    class AddCompanyCommand : CommandLineCommand
    {
        /// <summary>
        /// <para>Flag to differentiate this command from other command line commands</para>
        /// </summary>
        [KeyedArgument("-c", true, "company")]
        public string Flag = default;

        /// <summary>
        /// <para>Legal name of the company to add to the database</para>
        /// </summary>
        [PositionalArgument(0)]
        public string LegalName = default;

        /// <summary>
        /// Function to add the company specified by LegalName to the database, along with all the
        /// tables required for basic company functionality
        /// </summary>
        /// <param name="manipulator"><see cref="MySqlDataManipulator"/> used to add the company to the database</param>
        /// <remarks>As this is a command to be used by the developers on the project, error output is minimal</remarks>
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

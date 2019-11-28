using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;

namespace OldManInTheShopServer.Cli
{
    /// <summary>
    /// <see cref="CommandLineCommand"/> to change the company a user is registered with
    /// </summary>
    class ChangeUserCompanyCommand : CommandLineCommand
    {
        /// <summary>
        /// Flag used to differentiate this command from the other commands in this package
        /// </summary>
        [KeyedArgument("-u", true, "company")]
        public string Flag = default;

        /// <summary>
        /// Database id of the user to update
        /// </summary>
        [PositionalArgument(0)]
        public int UserId = default;

        /// <summary>
        /// Database id of the company to register the specified user to
        /// </summary>
        [PositionalArgument(1)]
        public int NewCompanyId = default;

        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            var user = manipulator.GetUserById(UserId);
            user.Company = NewCompanyId;
            if (!manipulator.UpdateUserCompany(user))
            {
                Console.WriteLine("User company switch failed");
                return;
            }
            Console.WriteLine("User company switching successful");
        }
    }
}

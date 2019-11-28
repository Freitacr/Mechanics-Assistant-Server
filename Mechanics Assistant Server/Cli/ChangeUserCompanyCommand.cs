using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;

namespace OldManInTheShopServer.Cli
{
    class ChangeUserCompanyCommand : CommandLineCommand
    {
        [KeyedArgument("-u", true, "company")]
        public string Flag = default;

        [PositionalArgument(0)]
        public int UserId = default;

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

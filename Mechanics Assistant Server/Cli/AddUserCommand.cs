using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Cli
{
    class AddUserCommand : CommandLineCommand
    {
        [KeyedArgument("-c", true, "user")]
        public string Flag;

        [PositionalArgument(0)]
        public string Email;
        
        [PositionalArgument(1)]
        public string Password;
        
        [PositionalArgument(2)]
        public string SecurityQuestion;
        
        [PositionalArgument(3)]
        public string SecurityAnswer;
        
        [PositionalArgument(4)]
        public int AccessLevel;

        [PositionalArgument(5)]
        public int CompanyId;


        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            if(!manipulator.AddUser(Email, Password, SecurityQuestion, SecurityAnswer, AccessLevel, CompanyId))
            {
                Console.WriteLine("Error while adding user...");
                return;
            }
            Console.WriteLine("Successfully added user with email " + Email);
        }
    }
}

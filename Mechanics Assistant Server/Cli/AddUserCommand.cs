using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Cli
{
    /// <summary>
    /// <see cref="CommandLineCommand"/> used to add a user to the database
    /// </summary>
    /// <remarks>One of the fields that is a part of this command is the AccessLevel
    /// This is particularly useful for creating admins on the fly</remarks>
    class AddUserCommand : CommandLineCommand
    {
        /// <summary>
        /// <para>Flag to differentiate this command from other commands in this package</para>
        /// </summary>
        [KeyedArgument("-c", true, "user")]
        public string Flag = default;

        /// <summary>
        /// <para>Email of the user to add to the database</para>
        /// </summary>
        [PositionalArgument(0)]
        public string Email = default;
        
        /// <summary>
        /// <para>Password of the user to add to the database</para>
        /// </summary>
        [PositionalArgument(1)]
        public string Password = default;
        
        /// <summary>
        /// <para>Question the user must answer to be authenticated after creation</para>
        /// </summary>
        [PositionalArgument(2)]
        public string SecurityQuestion = default;
        
        /// <summary>
        /// <para>Answer to the SecurityQuestion</para>
        /// </summary>
        [PositionalArgument(3)]
        public string SecurityAnswer = default;
        
        /// <summary>
        /// <para>AccessLevel of the user to add to the database.</para>
        /// For more information on access levels see <see cref="AccessLevelMasks"/>
        /// </summary>
        [PositionalArgument(4)]
        public int AccessLevel = default;

        /// <summary>
        /// <para>Database id of the company to register the user with</para>
        /// </summary>
        [PositionalArgument(5)]
        public int CompanyId = default;

        /// <summary>
        /// Function to create and add the user specified by the command to the database
        /// </summary>
        /// <param name="manipulator"><see cref="MySqlDataManipulator"/> used to add the user to the database</param>
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

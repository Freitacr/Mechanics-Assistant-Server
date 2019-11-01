using OldManInTheShopServer.Data.MySql;
using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Util
{
    class DatabaseEntityCreationUtilities
    {
        /// <summary>
        /// Analyzes the command lines stored in <paramref name="argumentsIn"/> and performs any company or user creation necessary
        /// </summary>
        /// <param name="manipulatorIn">Object to use to access the database if needed</param>
        /// <param name="argumentsIn">The command line arguments to analyze</param>
        /// <returns>True if there was a creation request made (thus the program should terminate), false otherwise (the program should continue onward)</returns>
        public static bool PerformRequestedCreation(MySqlDataManipulator manipulatorIn, CommandLineArgumentParser argumentsIn)
        {
            if (argumentsIn.KeyedArguments.ContainsKey("-c"))
            {
                if (argumentsIn.KeyedArguments["-c"].Equals("company"))
                {
                    if (!manipulatorIn.AddCompany(string.Join(" ", argumentsIn.PositionalArguments)))
                    {
                        Console.WriteLine("Failed to add company " + string.Join(" ", argumentsIn.PositionalArguments));
                        Console.WriteLine("Failed because of error " + manipulatorIn.LastException.Message);
                        return true;
                    }
                    Console.WriteLine("Successfully added company " + string.Join(" ", argumentsIn.PositionalArguments));
                    return true;
                }
                else if (argumentsIn.KeyedArguments["-c"].Equals("user"))
                {
                    if(argumentsIn.PositionalArguments.Count != 6 || !int.TryParse(argumentsIn.PositionalArguments[4], out int test) || (test & 1) == 0 || test < 0 || test > 15 || !int.TryParse(argumentsIn.PositionalArguments[5], out int test2))
                    {
                        Console.WriteLine("User creation format: -c user %email% %password% \"%security question%\" \"%security answer%\" %user_role% %company_id%\n" +
                            "user_role can take the form of any odd number from 1-15 inclusive");
                        return true;
                    }
                    Console.WriteLine("Attempting to add user with the email: " + argumentsIn.PositionalArguments[0]);
                    Console.WriteLine("Attempting to add user with the password: " + argumentsIn.PositionalArguments[1]);
                    Console.WriteLine("Attempting to add user with the security question: " + argumentsIn.PositionalArguments[2]);
                    Console.WriteLine("Attempting to add user with the security question answer: " + argumentsIn.PositionalArguments[3]);
                    Console.WriteLine("Attempting to add user with the access level: " + argumentsIn.PositionalArguments[4]);
                    Console.WriteLine("Attempting to add user to the company with the id: " + argumentsIn.PositionalArguments[5]);
                    string email = argumentsIn.PositionalArguments[0];
                    string password = argumentsIn.PositionalArguments[1];
                    string securityQuestion = argumentsIn.PositionalArguments[2];
                    string securityAnswer = argumentsIn.PositionalArguments[3];
                    int accessLevel = int.Parse(argumentsIn.PositionalArguments[4]);
                    int companyId = int.Parse(argumentsIn.PositionalArguments[5]);
                    if(!manipulatorIn.AddUser(
                        email,
                        password,
                        securityQuestion,
                        securityAnswer,
                        accessLevel,
                        companyId)
                    )
                    {
                        Console.WriteLine("Creation of user failed. Error that occurred:\n" + manipulatorIn.LastException);
                        return true;
                    }
                    Console.WriteLine("Successfully created user");
                    return true;
                }
                else
                {
                    Console.WriteLine("Only company and user creation is supported. Use -c company or -c user.");
                    return true;
                }
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using OldManInTheShopServer.Data;
using OldManInTheShopServer.Data.MySql;
using System.Text;

namespace MechanicsAssistantServerTests
{
    class TestingDatabaseCreationUtils
    {
        private static bool DatabaseInitialized = false;

        public static bool InitializeDatabaseSchema()
        {
            if (DatabaseInitialized)
                return true;
            MySqlDataManipulator manipulator = new MySqlDataManipulator();
            using (manipulator)
            {
                if (!manipulator.Connect(TestingConstants.DatabaselessConnectionString))
                {
                    Console.WriteLine("Encountered an error opening the global configuration connection");
                    Console.WriteLine(manipulator.LastException.Message);
                    return false;
                }
                if (!manipulator.ValidateDatabaseIntegrity(TestingConstants.DatabaselessConnectionString, "db_test"))
                {
                    Console.WriteLine("Encountered an error opening the global configuration connection");
                    Console.WriteLine(manipulator.LastException.Message);
                    return false;
                }
                if (!manipulator.Connect(TestingConstants.ConnectionString))
                {
                    Console.WriteLine("Encountered an error opening the global configuration connection");
                    Console.WriteLine(manipulator.LastException.Message);
                    return false;
                }
                if (manipulator.GetCompanyById(1) == null)
                {
                    if (!manipulator.AddCompany(TestingCompanyStorage.ValidCompany1))
                    {
                        Console.WriteLine("Encountered an error adding the first valid company");
                        Console.WriteLine(manipulator.LastException.Message);
                        return false;
                    }
                }
            }
            MySqlDataManipulator.GlobalConfiguration.Connect(TestingConstants.ConnectionString);
            MySqlDataManipulator.GlobalConfiguration.Close();
            DatabaseInitialized = true;
            return true;
        }

        public static bool InitializeUsers()
        {
            if (!DatabaseInitialized)
                return false;
            MySqlDataManipulator initializer = new MySqlDataManipulator();
            using (initializer)
            {
                if (!initializer.Connect(TestingConstants.ConnectionString))
                {
                    Console.WriteLine("Encountered an error opening the global configuration connection");
                    Console.WriteLine(initializer.LastException.Message);
                    return false;
                }
                if (initializer.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser1.Email)).Count == 0)
                {
                    if (!initializer.AddUser(
                            TestingUserStorage.ValidUser1.Email,
                            TestingUserStorage.ValidUser1.Password,
                            TestingUserStorage.ValidUser1.SecurityQuestion,
                            TestingUserStorage.ValidUser1.SecurityAnswer
                            )
                        )
                    {
                        Console.WriteLine("Encountered an error adding the first valid user.");
                        Console.WriteLine(initializer.LastException.Message);
                        return false;
                    }
                }
                return true;
            }
        }

        public static bool DestoryDatabase()
        {
            if (!DatabaseInitialized)
                return true;
            MySqlDataManipulator closer = new MySqlDataManipulator();
            using (closer)
            {
                closer.Connect(TestingConstants.ConnectionString);
                DatabaseInitialized = !closer.DropSchema("db_test");
            
                if (DatabaseInitialized)
                {
                    Console.WriteLine("Encountered an error dropping schema db_test");
                    Console.WriteLine(closer.LastException.Message);
                    closer.Close();
                    return false;
                }
            }
            return true;
        }
    }
}

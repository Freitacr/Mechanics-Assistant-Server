using System;
using System.Collections.Generic;
using OldManInTheShopServer.Data;
using OldManInTheShopServer.Data.MySql;
using System.Text;
using MySql.Data.MySqlClient;

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
                DatabaseInitialized = true;
                if (!InitializeUsers())
                    return false;
            }
            MySqlDataManipulator.GlobalConfiguration.Connect(TestingConstants.ConnectionString);
            MySqlDataManipulator.GlobalConfiguration.Close();
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
                            TestingUserStorage.ValidUser1.SecurityAnswer,
                            TestingUserStorage.ValidUser1.AccessLevel
                            )
                        )
                    {
                        Console.WriteLine("Encountered an error adding the first valid user.");
                        Console.WriteLine(initializer.LastException.Message);
                        return false;
                    }
                }
                if (initializer.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser2.Email)).Count == 0)
                {
                    if (!initializer.AddUser(
                            TestingUserStorage.ValidUser2.Email,
                            TestingUserStorage.ValidUser2.Password,
                            TestingUserStorage.ValidUser2.SecurityQuestion,
                            TestingUserStorage.ValidUser2.SecurityAnswer,
                            TestingUserStorage.ValidUser2.AccessLevel
                            )
                        )
                    {
                        Console.WriteLine("Encountered an error adding the second valid user.");
                        Console.WriteLine(initializer.LastException.Message);
                        return false;
                    }
                }
                if (initializer.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser3.Email)).Count == 0)
                {
                    if (!initializer.AddUser(
                            TestingUserStorage.ValidUser3.Email,
                            TestingUserStorage.ValidUser3.Password,
                            TestingUserStorage.ValidUser3.SecurityQuestion,
                            TestingUserStorage.ValidUser3.SecurityAnswer,
                            TestingUserStorage.ValidUser3.AccessLevel
                            )
                        )
                    {
                        Console.WriteLine("Encountered an error adding the second valid user.");
                        Console.WriteLine(initializer.LastException.Message);
                        return false;
                    }
                }
                if (initializer.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser4.Email)).Count == 0)
                {
                    if (!initializer.AddUser(
                            TestingUserStorage.ValidUser4.Email,
                            TestingUserStorage.ValidUser4.Password,
                            TestingUserStorage.ValidUser4.SecurityQuestion,
                            TestingUserStorage.ValidUser4.SecurityAnswer,
                            TestingUserStorage.ValidUser4.AccessLevel
                            )
                        )
                    {
                        Console.WriteLine("Encountered an error adding the second valid user.");
                        Console.WriteLine(initializer.LastException.Message);
                        return false;
                    }
                }
                if (initializer.GetUsersWhere(string.Format("Email=\"{0}\"", TestingUserStorage.ValidUser5.Email)).Count == 0)
                {
                    if (!initializer.AddUser(
                            TestingUserStorage.ValidUser5.Email,
                            TestingUserStorage.ValidUser5.Password,
                            TestingUserStorage.ValidUser5.SecurityQuestion,
                            TestingUserStorage.ValidUser5.SecurityAnswer,
                            TestingUserStorage.ValidUser5.AccessLevel
                            )
                        )
                    {
                        Console.WriteLine("Encountered an error adding the second valid user.");
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
            Console.WriteLine("Annihilating database");
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

    public class TestingMySqlConnectionUtil {
        public static bool DatabaseInitialized = false;
    
        public static void InitializeDatabaseSchema(MySqlConnection connectionIn, string tableCreationString, string tableName) {
            if(!DatabaseInitialized)
            {
                TestingDatabaseCreationUtils.InitializeDatabaseSchema();
                DatabaseInitialized = true;
            }
            connectionIn.ConnectionString = TestingConstants.ConnectionString;
            connectionIn.Open();
            var cmd = connectionIn.CreateCommand();
            cmd.CommandText = "select count(*) from information_schema.tables where table_name=\"{tableName}\"";
            var reader = cmd.ExecuteReader();
            int count = 0;
            using(reader) {
                reader.Read();
                count = reader.GetInt32(0);
            }
            if(count == 0)
            {
                cmd.CommandText = tableCreationString;
                cmd.ExecuteNonQuery();
            }
        }

        public static void DestoryDatabase() {
            TestingDatabaseCreationUtils.DestoryDatabase();
            DatabaseInitialized = false;
        }
    }

}

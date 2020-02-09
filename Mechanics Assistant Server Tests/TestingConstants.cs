using OldManInTheShopServer.Data.MySql;

namespace MechanicsAssistantServerTests
{
    class TestingConstants
    {
        public static readonly string ConnectionString = new MySqlConnectionString("localhost", "db_test", "testUser").ConstructConnectionString("");
        public static readonly string DatabaselessConnectionString = new MySqlConnectionString("localhost", null, "testUser").ConstructConnectionString("");
    }
}

namespace OldManInTheShopServer.Data.MySql
{
    /// <summary>
    /// Class for constructing a MySqlConnection's ConnectionString
    /// </summary>
    public class MySqlConnectionString
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string UserId { get; set; }
        
        public MySqlConnectionString()
        {

        }

        public MySqlConnectionString(string server, string database, string userid)
        {
            Server = server;
            Database = database;
            UserId = userid;
        }

        public string ConstructConnectionString(string password)
        {
            string ret = "SERVER=" + Server + ";";
            if(Database != null)
                ret += "DATABASE=" + Database + ";";
            ret += "UID=" + UserId + ";";
            ret += "PASSWORD=" + password + ";";
            return ret;
        }
    }
}

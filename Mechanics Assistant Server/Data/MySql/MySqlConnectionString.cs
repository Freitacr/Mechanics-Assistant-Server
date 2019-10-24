namespace OldManInTheShopServer.Data.MySql
{
    class MySqlConnectionString
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
            ret += "DATABASE=" + Database + ";";
            ret += "UID=" + UserId + ";";
            ret += "PASSWORD=" + password + ";";
            return ret;
        }
    }
}

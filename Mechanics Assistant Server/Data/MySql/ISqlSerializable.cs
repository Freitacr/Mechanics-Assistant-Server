using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace OldManinTheShopServer.Data.MySql
{
    interface ISqlSerializable
    {
        string Serialize(string tableName);
        void Deserialize(MySqlDataReader reader);
        ISqlSerializable Copy();
    }
}

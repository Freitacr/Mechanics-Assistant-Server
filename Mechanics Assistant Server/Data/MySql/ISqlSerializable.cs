using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace MechanicsAssistantServer.Data.MySql
{
    interface ISqlSerializable
    {
        string Serialize(string tableName);
        void Deserialize(MySqlDataReader reader);
        ISqlSerializable Copy();
    }
}

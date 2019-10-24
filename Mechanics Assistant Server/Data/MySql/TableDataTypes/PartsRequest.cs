using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    public class PartsRequest : ISqlSerializable
    {
        public static readonly TableDataManipulator<PartsRequest> Manipulator = new TableDataManipulator<PartsRequest>();

        public int UserId { get; set; }
        public string JobId { get; set; }
        public string ReferencedParts { get; set; }

        public PartsRequest()
        {

        }

        public PartsRequest(int userId, string jobId, string referencedParts)
        {
            UserId = userId;
            JobId = jobId;
            ReferencedParts = referencedParts;
        }


        public ISqlSerializable Copy()
        {
            return new PartsRequest(UserId, JobId, ReferencedParts);
        }

        public void Deserialize(MySqlDataReader reader)
        {
            UserId = (int)reader["UserId"];
            JobId = (string)reader["JobId"];
            ReferencedParts = (string)reader["ReferencedParts"];
        }

        public string Serialize(string tableName)
        {
            var retBuilder = new StringBuilder("insert into " + tableName);
            retBuilder.Append("(UserId, JobId, ReferencedParts) values(");
            retBuilder.Append(UserId.ToString());
            retBuilder.Append(",");
            retBuilder.Append("\"" + JobId + "\"");
            retBuilder.Append(",");
            retBuilder.Append("\"" + ReferencedParts + "\"");
            retBuilder.Append(");");
            return retBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return GetHashCode() == (obj as PartsRequest).GetHashCode();
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return UserId + ReferencedParts.GetHashCode() + JobId.GetHashCode();
        }
    }
}

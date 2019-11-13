using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    public class RequirementAdditionRequest : ISqlSerializable
    {
        public static readonly TableDataManipulator<RequirementAdditionRequest> Manipulator = new TableDataManipulator<RequirementAdditionRequest>();
        public int UserId { get; set; }
        public int ValidatedDataId { get; set; }
        public string RequestedAdditions { get; set; }

        public int Id { get; set; }

        public RequirementAdditionRequest()
        {

        }

        public RequirementAdditionRequest(int userId, int validatedDataId, string requestedAdditions)
        {
            UserId = userId;
            ValidatedDataId = validatedDataId;
            RequestedAdditions = requestedAdditions;
        }

        public ISqlSerializable Copy()
        {
            return new RequirementAdditionRequest(UserId, ValidatedDataId, RequestedAdditions);
        }

        public void Deserialize(MySqlDataReader reader)
        {
            UserId = (int)reader["UserId"];
            ValidatedDataId = (int)reader["ValidatedDataId"];
            RequestedAdditions = (string)reader["RequestedAdditions"];
            Id = (int)reader["id"];
        }

        public string Serialize(string tableName)
        {
            var retBuilder = new StringBuilder("insert into " + tableName);
            retBuilder.Append("(UserId, ValidatedDataId, RequestedAdditions) values(");
            retBuilder.Append(UserId);
            retBuilder.Append(",");
            retBuilder.Append(ValidatedDataId);
            retBuilder.Append(",");
            retBuilder.Append("\"" + RequestedAdditions.Replace("\"","\\\"") + "\"");
            retBuilder.Append(");");
            return retBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = obj as RequirementAdditionRequest;
            if (other.UserId != UserId)
                return false;
            if (other.ValidatedDataId != ValidatedDataId)
                return false;
            return other.RequestedAdditions == RequestedAdditions;
        }

        public override int GetHashCode()
        {
            return ValidatedDataId + UserId + RequestedAdditions.GetHashCode();
        }
    }
}

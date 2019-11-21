using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    public class PartsRequest : MySqlTableDataMember<PartsRequest>
    {
        public static readonly TableDataManipulator<PartsRequest> Manipulator = new TableDataManipulator<PartsRequest>();

        [SqlTableMember("int")]
        public int UserId;

        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string JobId;

        [SqlTableMember("varchar(256)", MySqlDataFormatString = "\"{0}\"")]
        public string ReferencedParts;

        public PartsRequest()
        {

        }

        public PartsRequest(int userId, string jobId, string referencedParts)
        {
            UserId = userId;
            JobId = jobId;
            ReferencedParts = referencedParts;
        }


        public override ISqlSerializable Copy()
        {
            return new PartsRequest(UserId, JobId, ReferencedParts);
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

        protected override void ApplyDefaults()
        {
        }

        public override string ToString()
        {
            return UserId.ToString() + (JobId ?? "");
        }
    }
}

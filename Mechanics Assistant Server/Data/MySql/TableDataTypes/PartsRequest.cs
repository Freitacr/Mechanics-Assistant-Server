using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    /// <summary>
    /// Class that represents a Part Request in the MySql database
    /// </summary>
    public class PartsRequest : MySqlTableDataMember<PartsRequest>
    {

        public static readonly TableDataManipulator<PartsRequest> Manipulator = new TableDataManipulator<PartsRequest>();

        /// <summary>
        /// Database id of the user that made the request
        /// </summary>
        [SqlTableMember("int")]
        public int UserId;

        /// <summary>
        /// The shop assigned id to the repair job that the part request is for
        /// </summary>
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string JobId;

        /// <summary>
        /// JSON formatted string of integers representing the database ids of the parts requested as a part of this request
        /// </summary>
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

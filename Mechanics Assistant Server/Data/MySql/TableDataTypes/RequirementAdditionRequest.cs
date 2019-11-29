using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    /// <summary>
    /// Class that represents a user's request to a company to add a requirement to a <see cref="RepairJobEntry"/>
    /// and how that is stored in the MySql database
    /// </summary>
    public class RequirementAdditionRequest : MySqlTableDataMember<RequirementAdditionRequest>
    {
        public static readonly TableDataManipulator<RequirementAdditionRequest> Manipulator = new TableDataManipulator<RequirementAdditionRequest>();

        /// <summary>
        /// Database id of the user that is requesting the requirement additions
        /// </summary>
        [SqlTableMember("int")]
        public int UserId;

        /// <summary>
        /// The database id of the <see cref="RepairJobEntry"/> in the user's registered company's validated data set
        /// </summary>
        [SqlTableMember("int")]
        public int ValidatedDataId;

        /// <summary>
        /// JSON formatted list representing the requirement additions the user is requesting to be added to the <see cref="RepairJobEntry"/>
        /// </summary>
        [SqlTableMember("varchar(256)", MySqlDataFormatString = "\"{0}\"")]
        public string RequestedAdditions;

        public RequirementAdditionRequest()
        {

        }

        public RequirementAdditionRequest(int userId, int validatedDataId, string requestedAdditions)
        {
            UserId = userId;
            ValidatedDataId = validatedDataId;
            RequestedAdditions = requestedAdditions;
        }

        public override ISqlSerializable Copy()
        {
            return new RequirementAdditionRequest(UserId, ValidatedDataId, RequestedAdditions);
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

        protected override void ApplyDefaults()
        {
        }

        public override string ToString()
        {
            return ValidatedDataId + ": " + (RequestedAdditions ?? "");
        }
    }
}

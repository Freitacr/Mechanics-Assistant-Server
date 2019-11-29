using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    /// <summary>
    /// <para>Class that represents a request from a user to join a company</para>
    /// <para>The company is not stored as part of this request as it is stored in the data table of the company
    /// the request is for</para>
    /// </summary>
    public class JoinRequest : MySqlTableDataMember<JoinRequest>
    {
        public static readonly TableDataManipulator<JoinRequest> Manipulator = new TableDataManipulator<JoinRequest>();
        
        /// <summary>
        /// Database id of the user that is requesting to join the company
        /// </summary>
        [SqlTableMember("int")]
        public int UserId;
        
        /// <summary>
        /// Default constructor, required by <see cref="TableDataManipulator{T}"/>
        /// </summary>
        public JoinRequest()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userId">Database id of the user who is making the request</param>
        public JoinRequest(int userId)
        {
            UserId = userId;
        }
        
        public override ISqlSerializable Copy()
        {
            return new JoinRequest(UserId);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return (obj as JoinRequest).UserId == UserId;
        }

        public override int GetHashCode()
        {
            return UserId;
        }

        protected override void ApplyDefaults()
        {
            
        }

        public override string ToString()
        {
            return UserId.ToString();
        }
    }
}

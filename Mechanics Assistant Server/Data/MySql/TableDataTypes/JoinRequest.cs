using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    public class JoinRequest : MySqlTableDataMember<JoinRequest>
    {
        public static readonly TableDataManipulator<JoinRequest> Manipulator = new TableDataManipulator<JoinRequest>();
        
        [SqlTableMember("int")]
        public int UserId;
        
        public JoinRequest()
        {

        }

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

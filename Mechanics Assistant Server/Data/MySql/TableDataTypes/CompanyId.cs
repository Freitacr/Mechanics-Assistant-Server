using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace MechanicsAssistantServer.Data.MySql.TableDataTypes
{
    class CompanyId : ISqlSerializable
    {
        public static readonly TableDataManipulator<CompanyId> Manipulator = new TableDataManipulator<CompanyId>();

        public string LegalName { get; set; }

        public CompanyId()
        {

        }

        public CompanyId(string legalName)
        {
            LegalName = legalName;
        }

        public ISqlSerializable Copy()
        {
            return new CompanyId(LegalName);
        }

        public void Deserialize(MySqlDataReader reader)
        {
            LegalName = (string)reader["LegalName"];
        }

        public string Serialize(string tableName)
        {
            return "insert into " + tableName + "(LegalName) values (\"" + LegalName + "\");";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return (obj as CompanyId).LegalName.Equals(LegalName);
        }

        public override int GetHashCode()
        {
            return LegalName.GetHashCode();
        }

        public override string ToString()
        {
            return LegalName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    public class CompanyId : ISqlSerializable
    {
        public static readonly TableDataManipulator<CompanyId> Manipulator = new TableDataManipulator<CompanyId>();

        public string LegalName { get; set; }
        public float ModelAccuracy { get; set; }
        public string LastTrainedTime { get; set; } = DateTime.MinValue.ToString();
        public string LastValidatedTime { get; set; } = DateTime.MinValue.ToString();
        public int Id { get; set; }
        

        public CompanyId()
        {

        }

        public CompanyId(string legalName, float modelAccuracy)
        {
            LegalName = legalName;
            ModelAccuracy = modelAccuracy;
        }

        public ISqlSerializable Copy()
        {
            return new CompanyId(LegalName, ModelAccuracy);
        }

        public void Deserialize(MySqlDataReader reader)
        {
            LegalName = (string)reader["LegalName"];
            ModelAccuracy = (float)reader["ModelAccuracy"];
            LastTrainedTime = (string)reader["LastTrainedTime"];
            LastValidatedTime = (string)reader["LastValidatedTime"];
            Id = (int)reader["id"];
        }

        public string Serialize(string tableName)
        {
            return "insert into " + tableName + "(LegalName, ModelAccuracy, LastTrainedTime, LastValidatedTime) values (\"" + LegalName + "\"," + ModelAccuracy.ToString() + "\"," + LastTrainedTime + "\"," + LastValidatedTime + "\");";
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

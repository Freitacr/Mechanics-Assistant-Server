using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    public class CompanyId : MySqlTableDataMember<CompanyId>
    {
        public static readonly TableDataManipulator<CompanyId> Manipulator = new TableDataManipulator<CompanyId>();

        [SqlTableMember("varchar(256)", MySqlDataFormatString = "\"{0}\"")]
        public string LegalName;

        [SqlTableMember("float")]
        public float ModelAccuracy;
        
        [SqlTableMember("varchar(64)", MySqlDataFormatString = "\"{0}\"")]
        public string LastTrainedTime = DateTime.MinValue.ToString();
        
        [SqlTableMember("varchar(64)", MySqlDataFormatString = "\"{0}\"")]
        public string LastValidatedTime = DateTime.MinValue.ToString();
        

        public CompanyId()
        {

        }

        public CompanyId(string legalName, float modelAccuracy)
        {
            LegalName = legalName;
            ModelAccuracy = modelAccuracy;
        }

        public override ISqlSerializable Copy()
        {
            return new CompanyId(LegalName, ModelAccuracy);
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

        protected override void ApplyDefaults()
        {
            LegalName = null;
            ModelAccuracy = 0;
            LastTrainedTime = DateTime.Now.ToString();
            LastValidatedTime = DateTime.Now.ToString();
        }
    }
}

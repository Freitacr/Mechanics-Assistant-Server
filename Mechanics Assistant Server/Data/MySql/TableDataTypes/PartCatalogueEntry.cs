using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    public class PartCatalogueEntry : MySqlTableDataMember<PartCatalogueEntry>
    {
        public static readonly TableDataManipulator<PartCatalogueEntry> Manipulator = new TableDataManipulator<PartCatalogueEntry>();

        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string Make;

        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string Model;

        [SqlTableMember("int")]
        public int Year = -1;

        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string PartId;

        [SqlTableMember("varchar(256)", MySqlDataFormatString = "\"{0}\"")]
        public string PartName;

        public PartCatalogueEntry()
        {

        }

        public PartCatalogueEntry(string make, string model, int year, string partId, string partName)
        {
            Make = make;
            Model = model;
            Year = year;
            PartId = partId;
            PartName = partName;
        }

        public override ISqlSerializable Copy()
        {
            return new PartCatalogueEntry(Make, Model, Year, PartId, PartName);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return (obj as PartCatalogueEntry).PartId.Equals(PartId);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return PartId.GetHashCode();
        }

        protected override void ApplyDefaults()
        {
            Year = -1;
        }

        public override string ToString()
        {
            return PartId ?? "";
        }
    }
}

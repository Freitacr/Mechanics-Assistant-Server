using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    /// <summary>
    /// Class representing the data of a Part in a company's part catalogue stored in the database
    /// </summary>
    public class PartCatalogueEntry : MySqlTableDataMember<PartCatalogueEntry>
    {
        public static readonly TableDataManipulator<PartCatalogueEntry> Manipulator = new TableDataManipulator<PartCatalogueEntry>();

        /// <summary>
        /// The make of the machine the part is for
        /// </summary>
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string Make;

        /// <summary>
        /// The model of the machine the part is for
        /// </summary>
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string Model;

        /// <summary>
        /// The year of the machine the part is for
        /// </summary>
        [SqlTableMember("int")]
        public int Year = -1;

        /// <summary>
        /// The real world part id string (usually looks like "ABCEF-XXX-XXX")
        /// </summary>
        [SqlTableMember("varchar(128)", MySqlDataFormatString = "\"{0}\"")]
        public string PartId;

        /// <summary>
        /// The more everyday name of the part this entry represents
        /// </summary>
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

using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace MechanicsAssistantServer.Data.MySql.TableDataTypes
{
    class PartCatalogueEntry : ISqlSerializable
    {
        public static readonly TableDataManipulator<PartCatalogueEntry> Manipulator = new TableDataManipulator<PartCatalogueEntry>();
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string PartId { get; set; }
        public string PartName { get; set; }

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

        public ISqlSerializable Copy()
        {
            return new PartCatalogueEntry(Make, Model, Year, PartId, PartName);
        }

        public void Deserialize(MySqlDataReader reader)
        {
            Make = (string)reader["Make"];
            Model = (string)reader["Model"];
            Year = (int)reader["Year"];
            PartId = (string)reader["PartId"];
            PartName = (string)reader["PartName"];
        }

        public string Serialize(string tableName)
        {
            return "insert into " + tableName + " (Make, Model, Year, PartId, PartName) values (\"" +
                Make + "\",\"" + Model + "\"," + Year + ",\"" + PartId + "\",\"" + PartName + "\");";
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
    }
}

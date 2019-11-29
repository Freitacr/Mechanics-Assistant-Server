using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Attribute;

namespace OldManInTheShopServer.Data.MySql.TableDataTypes
{
    /// <summary>
    /// Class that represents a Company in the MySQL Database
    /// </summary>
    public class CompanyId : MySqlTableDataMember<CompanyId>
    {
        public static readonly TableDataManipulator<CompanyId> Manipulator = new TableDataManipulator<CompanyId>();

        /// <summary>
        /// Company's Legal Name
        /// </summary>
        [SqlTableMember("varchar(256)", MySqlDataFormatString = "\"{0}\"")]
        public string LegalName;

        /// <summary>
        /// <para>Field representing the current accuracy of the clustering models based on automated testing</para>
        /// <para>For more information on automated testing see <see cref="Util.CompanyModelUtils.PerformAutomatedTesting"/></para>
        /// </summary>
        [SqlTableMember("float")]
        public float ModelAccuracy;

        /// <summary>
        /// <para>UTC string that represents the last time this company had its clustering models trained</para>
        /// <para>For more information on automated training see <see cref="Util.CompanyModelUtils.TrainClusteringModel"/></para>
        /// </summary>
        [SqlTableMember("varchar(64)", MySqlDataFormatString = "\"{0}\"")]
        public string LastTrainedTime = DateTime.MinValue.ToString();

        /// <summary>
        /// <para>UTC string that represents the last time this company had its data validated</para>
        /// <para>For more information on data validation see <see cref="Util.CompanyModelUtils.PerformDataValidation"/></para>
        /// </summary>
        [SqlTableMember("varchar(64)", MySqlDataFormatString = "\"{0}\"")]
        public string LastValidatedTime = DateTime.MinValue.ToString();
        
        /// <summary>
        /// Default constructor, required by <see cref="TableDataManipulator{T}"/>
        /// </summary>
        public CompanyId()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="legalName">Legal name of the company</param>
        /// <param name="modelAccuracy">Initial accuracy of the clustering model for the company (usually is 0.0)</param>
        public CompanyId(string legalName, float modelAccuracy)
        {
            LegalName = legalName;
            ModelAccuracy = modelAccuracy;
        }

        /// <summary>
        /// Returns a copy of the current object. This copy is shallow
        /// </summary>
        /// <returns>CompanyId object containing the same data as this one</returns>
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

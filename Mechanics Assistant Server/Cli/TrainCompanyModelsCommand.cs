using System;
using System.Collections.Generic;
using System.Linq;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Models;
using OldManInTheShopServer.Models.KeywordClustering;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Cli
{
    /// <summary>
    /// <see cref="CommandLineCommand"/> used to train the <see cref="IDatabaseKeywordClusterer"/> for the company
    /// specified by <see cref="CompanyId"/>
    /// </summary>
    class TrainCompanyModelsCommand : CommandLineCommand
    {
        /// <summary>
        /// <para>Flag used to differentiate this command from the other commands in this package</para>
        /// The only valid flag value for this command is "complaint"
        /// </summary>
        [KeyedArgument("-t")]
        public string Flag = default;

        /// <summary>
        /// Database id of the company to train the clustering models for
        /// </summary>
        [PositionalArgument(0)]
        public int CompanyId = default;

        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            //Ensure that all KeywordPredictor models are loaded
            //If one is not, then a company requesting that model through its settings will cause an error
            if (!GlobalModelHelper.LoadOrTrainGlobalModels(ReflectionHelper.GetAllKeywordPredictors()))
                throw new NullReferenceException("One or more global models failed to load. Server cannot start.");
            DatabaseQueryProcessor processor = new DatabaseQueryProcessor(DatabaseQueryProcessorSettings.RetrieveCompanySettings(manipulator, CompanyId));

            List<RepairJobEntry> validatedData = manipulator.GetDataEntriesWhere(CompanyId, "id > 0", validated: true);
            List<string> sentences;
            if (Flag.ToLower().Equals("complaint"))
            {
                //train model
                sentences = validatedData.Select(entry => entry.Complaint).ToList();
                if (!processor.TrainClusteringModels(manipulator, CompanyId, sentences, false))
                {
                    Console.WriteLine("Failed to train problem prediction models for company " + CompanyId);
                    return;
                }
                //register the complaint groups that the clusterer predicts with the repair job entry in the database
                foreach (RepairJobEntry entry in validatedData)
                {
                    string groups = JsonDataObjectUtil<List<int>>.ConvertObject(processor.PredictGroupsInJobData(entry, CompanyId, manipulator));
                    entry.ComplaintGroups = groups;
                    manipulator.UpdateDataEntryGroups(CompanyId, entry, complaint: true);
                }
            }
            Console.WriteLine("Trained clustering models for company " + CompanyId);
        }
    }
}

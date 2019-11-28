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
    class TrainCompanyModelsCommand : CommandLineCommand
    {

        [KeyedArgument("-t")]
        public string Flag = default;

        [PositionalArgument(0)]
        public int CompanyId = default;

        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            if (!GlobalModelHelper.LoadOrTrainGlobalModels(ReflectionHelper.GetAllKeywordPredictors()))
                throw new NullReferenceException("One or more global models failed to load. Server cannot start.");
            DatabaseQueryProcessor processor = new DatabaseQueryProcessor(DatabaseQueryProcessorSettings.RetrieveCompanySettings(manipulator, CompanyId));

            List<JobDataEntry> validatedData = manipulator.GetDataEntriesWhere(CompanyId, "id > 0", validated: true);
            List<string> sentences;
            if (Flag.ToLower().Equals("complaint"))
            {
                sentences = validatedData.Select(entry => entry.Complaint).ToList();
                if (!processor.TrainClusteringModels(manipulator, CompanyId, sentences, false))
                {
                    Console.WriteLine("Failed to train problem prediction models for company " + CompanyId);
                    return;
                }
                foreach (JobDataEntry entry in validatedData)
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

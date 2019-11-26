using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace OldManInTheShopServer.Util
{
    [DataContract]
    class ComplaintGroupJson
    {
        [DataMember]
        public string GroupDefinition;

        [DataMember]
        public int Id;
    }

    [DataContract]
    class SimilarQueryJson
    {
        [DataMember]
        public string Make;

        [DataMember]
        public string Model;

        [DataMember]
        public string Problem;

        [DataMember]
        public string Complaint;
    }

    class NonValidatedMapping
    {
        public JobDataEntry Entry;
        public double Vote;
    }

    class AccuracyMapping
    {
        public List<NonValidatedMapping> Mapping;
        public double Accuracy;
    }

    class CompanyModelUtils
    {

        public static void TrainClusteringModel(MySqlDataManipulator manipulator, DatabaseQueryProcessor processor, int companyId, bool training = false)
        {
            List<JobDataEntry> validatedData = manipulator.GetDataEntriesWhere(companyId, "id > 0", validated: true);
            List<string> sentences;
            sentences = validatedData.Select(entry => entry.Complaint).ToList();
            if (!processor.TrainClusteringModels(manipulator, companyId, sentences, training))
            {
                Console.WriteLine("Failed to train problem prediction models for company " + companyId);
                return;
            }
            if(!training)
                foreach (JobDataEntry entry in validatedData)
                {
                    string groups = JsonDataObjectUtil<List<int>>.ConvertObject(processor.PredictGroupsInJobData(entry, companyId, manipulator));
                    entry.ComplaintGroups = groups;
                    manipulator.UpdateDataEntryGroups(companyId, entry, complaint: true);
                }
        }

        public static double PerformAutomatedTesting(MySqlDataManipulator manipulator, int companyId, DatabaseQueryProcessor processor)
        {
            List<JobDataEntry> validatedData = manipulator.GetDataEntriesWhere(companyId, "id > 0", validated: true);
            return PerformAutomatedTestingWithData(manipulator, companyId, processor, validatedData);
        }

        public static double PerformAutomatedTestingWithData(MySqlDataManipulator manipulator, int companyId, DatabaseQueryProcessor processor, List<JobDataEntry> entries)
        {
            double currentDifference = 0;
            foreach (JobDataEntry entry in entries)
            {
                JobDataEntry testEntryCopy = new JobDataEntry()
                {
                    Make = entry.Make,
                    Model = entry.Model,
                    Year = entry.Year,
                    Complaint = entry.Complaint
                };
                
                List<string> entryProblemKeywords = processor.PredictKeywordsInJobData(entry, false);
                int startingNumKeywords = entryProblemKeywords.Count;
                string complaintGroupsJson = processor.ProcessQueryForComplaintGroups(testEntryCopy, manipulator, companyId);
                List<ComplaintGroupJson> complaintGroups = JsonDataObjectUtil<List<ComplaintGroupJson>>.ParseObject(complaintGroupsJson);
                List<int> complaintGroupIds = complaintGroups.Select(group => { return group.Id; }).ToList();
                foreach (int complaintGroupId in complaintGroupIds)
                {
                    string dataEntryJson = processor.ProcessQueryForSimilarQueries(testEntryCopy, manipulator, companyId, complaintGroupId, 5);
                    List<SimilarQueryJson> jobDataEntries = JsonDataObjectUtil<List<SimilarQueryJson>>.ParseObject(dataEntryJson);
                    List<JobDataEntry> dataEntries = jobDataEntries.Select(query => new JobDataEntry() { Make = query.Make, Model = query.Model, Problem = query.Problem, Complaint = query.Complaint }).ToList();
                    foreach (JobDataEntry currEntry in dataEntries)
                    {
                        List<string> currEntryKeywords = processor.PredictKeywordsInJobData(currEntry, false);
                        List<string> toRemove = entryProblemKeywords.Where(keyword => currEntryKeywords.Contains(keyword)).ToList();
                        foreach (string keyword in toRemove)
                            entryProblemKeywords.Remove(keyword);
                    }
                }
                currentDifference += (double)entryProblemKeywords.Count / startingNumKeywords;
            }
            return (currentDifference / entries.Count) * 100;
        }

        public static void PerformDataValidation(MySqlDataManipulator manipulator, int companyId, DatabaseQueryProcessor processor, int numShuffleTests = 5, int numGroups = 3)
        {
            List<JobDataEntry> validatedData = manipulator.GetDataEntriesWhere(companyId, "id>0", validated: true);
            List<JobDataEntry> nonValidatedData = manipulator.GetDataEntriesWhere(companyId, "id>0", validated: false);
            List<NonValidatedMapping> mappings = nonValidatedData.Select(entry => new NonValidatedMapping() { Entry = entry, Vote = 0 }).ToList();
            double currCompanyAccuracy = manipulator.GetCompanyAccuracy(companyId);
            for(int i = 0; i < numShuffleTests; i++)
            {
                mappings.Shuffle();
                List<List<NonValidatedMapping>> nonValidatedTestingGroups = mappings.Split(numGroups);
                foreach(List<NonValidatedMapping> currentTestGroup in nonValidatedTestingGroups)
                {
                    List<JobDataEntry> testGroup = new List<JobDataEntry>(validatedData);
                    testGroup.AddRange(currentTestGroup.Select(mapping => mapping.Entry));
                    processor.TrainClusteringModels(manipulator, companyId, testGroup.Select(entry => entry.Complaint).ToList(), training: true);
                    double accuracy = 100 - PerformAutomatedTestingWithData(manipulator, companyId, processor, testGroup);
                    double vote = (accuracy - currCompanyAccuracy) / currCompanyAccuracy;
                    foreach (NonValidatedMapping mapping in currentTestGroup)
                        mapping.Vote += vote;
                }
            }
            bool changed = false;
            foreach(NonValidatedMapping mapping in mappings)
            {
                if(mapping.Vote > 0.01)
                {
                    if(!manipulator.UpdateValidationStatus(companyId, mapping.Entry, wasValidated: false))
                    {
                        Console.WriteLine("Failed to update validation status of Repair Job Entry: " + mapping.Entry.Serialize(TableNameStorage.CompanyNonValidatedRepairJobTable.Replace("(n)", companyId.ToString())));
                        continue;
                    }
                    changed = true;
                }
            }
            if(changed)
            {
                TrainClusteringModel(manipulator, processor, companyId, false);
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OldManInTheShopServer.Models;
using OldManInTheShopServer.Models.KeywordPrediction;
using OldManInTheShopServer.Models.QueryProblemPrediction;
using OldManInTheShopServer.Models.KeywordClustering;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Models.POSTagger;
using OldManInTheShopServer.Util;
using OldManInTheShopServer.Data.MySql;

namespace OldManInTheShopServer.Models
{
    public class DatabaseQueryProcessorSettings
    {
        public string TaggerFilePath;
        public string KeywordPredictorFilePath;

        public string KeywordPredictorIdString;
        public string KeywordClustererIdString;
        public string ProblemPredictorIdString;
    
        public static DatabaseQueryProcessorSettings GenerateDefaultSettings()
        {
            DatabaseQueryProcessorSettings ret = new DatabaseQueryProcessorSettings()
            {
                TaggerFilePath = "InitialData/averaged_perceptron_tagger.ans",
                KeywordPredictorFilePath = "InitialData/nb_keyword_predictor.ans",
                KeywordClustererIdString = "DatabaseKeywordSimilarityClusterer",
                KeywordPredictorIdString = "NaiveBayesKeywordPredictor",
                ProblemPredictorIdString = "DatabaseQueryProblemPredictor"
            };
            return ret;
        }

        public static DatabaseQueryProcessorSettings RetrieveCompanySettings(MySqlDataManipulator manipulator, int companyId)
        {
            DatabaseQueryProcessorSettings companySettings = new DatabaseQueryProcessorSettings();
            List<CompanySettingsEntry> settings = manipulator.GetCompanySettings(companyId);
            foreach (CompanySettingsEntry setting in settings)
            {
                if (CompanySettingsKey.KeywordClusterer.Equals(setting.SettingKey))
                {
                    if (setting.SettingValue.Equals("Similarity"))
                    {
                        companySettings.KeywordClustererIdString = "DatabaseKeywordSimilarityClusterer";
                    }
                    else
                    {
                        companySettings.KeywordClustererIdString = "DatabaseKeywordSimilarityClusterer";
                    }
                }
                else if (CompanySettingsKey.KeywordPredictor.Equals(setting.SettingKey))
                {
                    if (setting.SettingValue.Equals("Bayesian"))
                    {
                        companySettings.KeywordPredictorIdString = "NaiveBayesKeywordPredictor";
                    }
                    else
                    {
                        companySettings.KeywordPredictorIdString = "NaiveBayesKeywordPredictor";
                    }
                }
                else if (CompanySettingsKey.ProblemPredictor.Equals(setting.SettingKey))
                {
                    if (setting.SettingValue.Equals("Database"))
                    {
                        companySettings.ProblemPredictorIdString = "DatabaseQueryProblemPredictor";
                    }
                    else
                    {
                        companySettings.ProblemPredictorIdString = "DatabaseQueryProblemPredictor";
                    }
                }
            }
            return companySettings;
        }
    }

    /// <summary>
    /// Class responsible for processing Prediction Queries using the MySql Database
    /// </summary>
    public class DatabaseQueryProcessor
    {
        private AveragedPerceptronTagger KeywordTagger;
        private IKeywordPredictor KeywordPredictor;
        private IDatabaseQueryProblemPredictor ProblemPredictor;
        private IDatabaseKeywordClusterer KeywordClusterer;
        

        public DatabaseQueryProcessor(DatabaseQueryProcessorSettings settingsIn = null)
        {
            if (settingsIn == null)
                settingsIn = DatabaseQueryProcessorSettings.GenerateDefaultSettings();

            KeywordTagger = AveragedPerceptronTagger.GetTagger();
            
            var keywordPredictorType = Type.GetType("OldManInTheShopServer.Models.KeywordPrediction."+settingsIn.KeywordPredictorIdString);
            if (keywordPredictorType == null)
                throw new NullReferenceException("Type string " + settingsIn.KeywordPredictorIdString + " was not found matching a known class in KeywordPrediction");
            try
            {
                KeywordPredictor = keywordPredictorType.GetMethod("GetGlobalModel").Invoke(null, null) as IKeywordPredictor;
            } catch (NullReferenceException e)
            {
                Console.WriteLine("Type " + settingsIn.KeywordPredictorIdString + " did not contain a GetGlobalModel method. It needs to have one.");
                throw e;
            }
            if (KeywordPredictor == null)
                throw new InvalidCastException("Loaded class could not be cast to IKeywordPredictor");
            
            var keywordClustererType = Type.GetType("OldManInTheShopServer.Models.KeywordClustering." + settingsIn.KeywordClustererIdString);
            if (keywordClustererType == null)
                throw new NullReferenceException("Type string " + settingsIn.KeywordClustererIdString + " was not found matching a known class in KeywordClustering");
            KeywordClusterer = keywordClustererType.GetConstructor(new Type[0]).Invoke(null) as IDatabaseKeywordClusterer;
            if (KeywordClusterer == null)
                throw new InvalidCastException("Loaded class could not be cast to IDatabaseKeywordClusterer");

            var problemPredictorType = Type.GetType("OldManInTheShopServer.Models.QueryProblemPrediction." + settingsIn.ProblemPredictorIdString);
            if (keywordPredictorType == null)
                throw new NullReferenceException("Type string " + settingsIn.ProblemPredictorIdString + " was not found matching a known class in QueryProblemPrediction");
            ProblemPredictor = problemPredictorType.GetConstructor(new Type[0]).Invoke(null) as IDatabaseQueryProblemPredictor;
            if (ProblemPredictor == null)
                throw new InvalidCastException("Loaded class could not be cast to IDatabaseQueryProblemPredictor");
        }

        public bool TrainClusteringModels(MySqlDataManipulator manipulator, int companyId, List<string> examplesIn, bool complaint = true)
        {
            List<KeywordExample> trainingData = new List<KeywordExample>();
            foreach(string sentence in examplesIn)
            {
                List<string> tokens = SentenceTokenizer.TokenizeSentence(sentence);
                List<List<string>> taggedTokens = KeywordTagger.Tag(tokens);
                List<string> keywords = KeywordPredictor.PredictKeywords(taggedTokens);
                KeywordExample example = new KeywordExample();
                foreach (string keyword in keywords)
                    example.AddKeyword(keyword);
                trainingData.Add(example);
            }
            KeywordClusterer.Train(trainingData);
            return KeywordClusterer.Save(manipulator, companyId, complaint);
        }

        public List<string> PredictKeywordsInJobData(JobDataEntry entry, bool complaint = true)
        {
            List<string> tokens;
            if (complaint)
                tokens = SentenceTokenizer.TokenizeSentence(entry.Complaint);
            else
                tokens = SentenceTokenizer.TokenizeSentence(entry.Problem);
            List<List<string>> taggedTokens = KeywordTagger.Tag(tokens);
            return KeywordPredictor.PredictKeywords(taggedTokens);
        }

        public List<int> PredictGroupsInJobData(JobDataEntry entry, int companyId, MySqlDataManipulator manipulator, bool complaint = true)
        {
            List<string> keywords = PredictKeywordsInJobData(entry, complaint);
            KeywordExample example = new KeywordExample();
            foreach (string keyword in keywords)
                example.AddKeyword(keyword);
            KeywordClusterer.Load(manipulator, companyId);
            List<int> groups = KeywordClusterer.PredictTopNSimilarGroups(example, 5);
            return groups;
        }

        /// <summary>
        /// Attempts to return a list of the top 3 most similar complaint groups from the database
        /// </summary>
        /// <param name="entryIn">The query to predict the most similar complaint groups of</param>
        /// <param name="manipulator">The object to use to access the database</param>
        /// <param name="companyId">The id of the company the request is being made for. Determines which tables to use in the database</param>
        /// <returns>Json formatted string that contains the top 3 complaint groups that are most similar to the query made, and their database ids</returns>
        public string ProcessQueryForComplaintGroups(JobDataEntry entryIn, MySqlDataManipulator manipulator, int companyId, int numGroupsRequested=3)
        {
            List<string> tokens = SentenceTokenizer.TokenizeSentence(entryIn.Complaint);
            List<List<string>> taggedTokens = KeywordTagger.Tag(tokens);
            List<string> keywords = KeywordPredictor.PredictKeywords(taggedTokens);
            KeywordExample example = new KeywordExample();
            foreach (string keyword in keywords)
                example.AddKeyword(keyword);
            KeywordClusterer.Load(manipulator, companyId);
            List<int> groups = KeywordClusterer.PredictTopNSimilarGroups(example, numGroupsRequested);
            List<KeywordGroupEntry> companyComplaintGroups = manipulator.GetCompanyComplaintGroups(companyId);
            if (companyComplaintGroups == null)
                throw new NullReferenceException("Company " + companyId + " complaint groups were not available in database");
            List<KeywordGroupEntry> ret = new List<KeywordGroupEntry>();
            bool uncategorizedAdded = false;
            foreach(int i in groups)
            {
                if (i == 0 && !uncategorizedAdded)
                {
                    ret.Add(new KeywordGroupEntry("Uncategorized") { Id = 0 });
                    uncategorizedAdded = true;
                }
                else if (i != 0)
                    ret.Add(companyComplaintGroups[i-1]);
            }
            JsonListStringConstructor constructor = new JsonListStringConstructor();
            ret.ForEach(obj => constructor.AddElement(ConvertKeywordGroupEntry(obj)));
            return constructor.ToString();

            JsonDictionaryStringConstructor ConvertKeywordGroupEntry(KeywordGroupEntry e)
            {
                JsonDictionaryStringConstructor r = new JsonDictionaryStringConstructor();
                r.SetMapping("GroupDefinition", e.GroupDefinition);
                r.SetMapping("Id", e.Id);
                return r;
            }
        }

        /// <summary>
        /// Attempts to retrieve the top <paramref name="numRequested"/> similar JobDataEntries from the database that are a part of the specified complaint group
        /// </summary>
        /// <param name="entryIn">Entry that represents the query made</param>
        /// <param name="manipulator">Object to access the database with</param>
        /// <param name="companyId">The id of the company to make the request of. Determines which tables to retrieve the data from</param>
        /// <param name="complaintGroupId">Database id of the complaint group to match JobDataEntries by</param>
        /// <param name="numRequested">Number of requested JobDataEntries to output</param>
        /// <param name="offset">Number to offset the list of returned JobDataEntries by.
        /// So with an offset of 5 and 10 JobDataEntires requested, the top 5-15 JobDataEntries would instead be returned</param>
        /// <returns>Json string containing the requested similar JobDataEntries</returns>
        public string ProcessQueryForSimilarQueries(JobDataEntry entryIn, MySqlDataManipulator manipulator, int companyId, int complaintGroupId, int numRequested, int offset=0)
        {
            List<string> tokens = SentenceTokenizer.TokenizeSentence(entryIn.Complaint);
            List<List<string>> taggedTokens = KeywordTagger.Tag(tokens);
            List<string> keywords = KeywordPredictor.PredictKeywords(taggedTokens);
            KeywordExample example = new KeywordExample();
            foreach (string keyword in keywords)
                example.AddKeyword(keyword);
            KeywordClusterer.Load(manipulator, companyId);
            List<int> groups = KeywordClusterer.PredictTopNSimilarGroups(example, 3);
            entryIn.ComplaintGroups = "[" + string.Join(',', groups) + "]";
            List<JobDataEntry> potentials = manipulator.GetDataEntriesByComplaintGroup(companyId, complaintGroupId);
            List<EntrySimilarity> ret = ProblemPredictor.GetQueryResults(entryIn, potentials, numRequested, offset);
            JsonListStringConstructor retConstructor = new JsonListStringConstructor();
            ret.ForEach(obj => retConstructor.AddElement(ConvertEntrySimilarity(obj)));
            return retConstructor.ToString();


            JsonDictionaryStringConstructor ConvertEntrySimilarity(EntrySimilarity e)
            {
                JsonDictionaryStringConstructor r = new JsonDictionaryStringConstructor();
                r.SetMapping("Make", e.Entry.Make);
                r.SetMapping("Model", e.Entry.Model);
                r.SetMapping("Complaint", e.Entry.Complaint);
                r.SetMapping("Problem", e.Entry.Problem);
                if (e.Entry.Year == -1)
                    r.SetMapping("Year", "Unknown");
                else
                    r.SetMapping("Year", e.Entry.Year);
                r.SetMapping("Id", e.Entry.Id);
                r.SetMapping("Difference", e.Difference);
                return r;
            }
        }

        


    }
}

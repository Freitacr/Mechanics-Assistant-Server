using System;
using System.Collections.Generic;
using System.Text;
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
    }

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

        public List<string> ProcessQueryForComplaintGroups(JobDataEntry entryIn, MySqlDataManipulator manipulator, int companyId)
        {
            List<string> tokens = SentenceTokenizer.TokenizeSentence(entryIn.Complaint);
            List<List<string>> taggedTokens = KeywordTagger.Tag(tokens);
            List<string> keywords = KeywordPredictor.PredictKeywords(taggedTokens);
            KeywordExample example = new KeywordExample();
            foreach (string keyword in keywords)
                example.AddKeyword(keyword);
            List<int> groups = KeywordClusterer.PredictTopNSimilarGroups(example, 3);
            List<KeywordGroupEntry> companyComplaintGroups = manipulator.GetCompanyComplaintGroups(companyId);
            if (companyComplaintGroups == null)
                throw new NullReferenceException("Company " + companyId + " complaint groups were not available in database");
            List<string> ret = new List<string>();
            foreach(int i in groups)
            {
                if (i == 0)
                    ret.Add("Uncategorized");
                else
                    ret.Add(companyComplaintGroups[i + 1].GroupDefinition);
            }
            return ret;
        }

    }
}

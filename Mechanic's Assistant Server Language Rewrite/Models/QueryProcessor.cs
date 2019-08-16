﻿using System;
using System.Collections.Generic;
using System.IO;
using MechanicsAssistantServer.Models.POSTagger;
using MechanicsAssistantServer.Models.KeywordPrediction;
using MechanicsAssistantServer.Models.KeywordClustering;
using MechanicsAssistantServer.Models.QueryProblemPrediction;
using MechanicsAssistantServer.Util;

namespace MechanicsAssistantServer.Models
{
    public class QueryProcessorSettings
    {
        public string TaggerFilePath;
        public string KeywordPredictorFilePath;
        public string KeywordClustererFilePath;
        public string ProblemPredictorFilePath;

        public string KeywordPredictorIdString;
        public string KeywordClustererIdString;
        public string ProblemPredictorIdString;

        public bool IsComplete()
        {
            return !(TaggerFilePath == null ||
                KeywordPredictorFilePath == null ||
                KeywordClustererFilePath == null ||
                ProblemPredictorFilePath == null ||
                KeywordPredictorIdString == null ||
                KeywordClustererIdString == null ||
                ProblemPredictorIdString == null);
        }

        public static QueryProcessorSettings GenerateDefaultSettings()
        {
            QueryProcessorSettings ret = new QueryProcessorSettings();
            ret.TaggerFilePath = "";
            ret.KeywordPredictorFilePath = "";
            ret.KeywordPredictorIdString = "";
            ret.KeywordClustererFilePath = "";
            ret.KeywordClustererIdString = "";
            ret.ProblemPredictorFilePath = "";
            ret.ProblemPredictorIdString = "";
            return ret;
        }
    }

    public class QueryProcessor
    {
        private static readonly int NUMBER_COMPLAINT_GROUPS = 3;
        private static readonly int NUMBER_QUERIES_OUT = 10;

        private AveragedPerceptronTagger PartOfSpeechTagger;
        private IKeywordPredictor KeywordPredictor;
        private IKeywordClusterer KeywordClusterer;
        private IQueryProblemPredictor ProblemPredictor;

        public QueryProcessor(QueryProcessorSettings settingsIn)
        {
            if (!settingsIn.IsComplete())
                throw new ArgumentException("Settings was not fully filled out prior to attempted use");

            if (!LoadModels(settingsIn))
                RestoreModels();
        }

        private static double CalculateDistance(List<double> x, List<double> y)
        {
            if (x.Count != y.Count)
                throw new ArgumentException("Data point counts must be identical to calculate distance!");
            double ret = 0;
            ret += x[0] == y[0] ? 0 : 1;
            ret += x[1] == y[1] ? 0 : 1;
            for (int i = 2; i < x.Count; i++)
                ret += Math.Pow(y[i] - x[i], 2);
            return Math.Sqrt(ret);
        }

        public List<string> ProcessQuery(MechanicQuery queryIn)
        {
            List<List<string>> complaintTokens = PartOfSpeechTagger.Tag(
                SentenceTokenizer.TokenizeSentence(queryIn.Complaint)
            );
            List<string> keywords = KeywordPredictor.PredictKeywords(complaintTokens);
            KeywordExample ex = new KeywordExample();
            foreach (string s in keywords)
                ex.AddKeyword(s);
            List<int> complaintGroups = KeywordClusterer.PredictTopNSimilarGroups(ex, NUMBER_COMPLAINT_GROUPS);
            List<object> queryDataPoint = new List<object> { queryIn.Make, queryIn.Model };
            foreach (int x in complaintGroups)
                queryDataPoint.Add(x);
            List<object> predictedProblems = ProblemPredictor.PredictTopN(queryDataPoint, CalculateDistance, NUMBER_QUERIES_OUT);
            List<string> returnProblems = new List<string>();
            foreach (object o in predictedProblems)
                returnProblems.Add((string)o);
            return returnProblems;
        }

        private bool LoadModels(QueryProcessorSettings settingsIn)
        {
            bool ret = LoadPartOfSpeechTagger(DefaultModelFileLocations.POS_TAGGER_ENG_FILE);
            ret &= LoadKeywordPredictor(DefaultModelFileLocations.NAIVE_BAYES_KEYWORD_PREDICTOR_FILE);
            ret &= LoadKeywordClusterer(DefaultModelFileLocations.KEYWORD_SIMILARITY_CLUSTERER_FILE);
            ret &= LoadProblemPredictor(DefaultModelFileLocations.KNN_QUERY_PROBLEM_PREDICTOR_FILE);
            return ret;
        }

        private bool LoadPartOfSpeechTagger(string filePath)
        {
            try
            {
                PartOfSpeechTagger = AveragedPerceptronTagger.Load(filePath);
            }
            catch (IOException)
            {
                return false;
            }
            return true;
        }

        /**
         * Summary:
         *  Loads the the currently selected KeywordPredictor from the file specified
         * Note:
         *  This method does not do the above currently. It is hard coded to use the NaiveBayesKeywordPredictor.
         *  This is a hold out until the proper method is in place to switch between implementations
         *  through a configuration file.
         */
        private bool LoadKeywordPredictor(string filePath)
        {
            KeywordPredictor = new NaiveBayesKeywordPredictor();
            try
            {
                KeywordPredictor.Load(filePath);
            } catch (IOException)
            {
                KeywordPredictor = null;
                return false;
            }
            return true;
        }

        /**
         * Summary:
         *  Loads the the currently selected KeywordClusterer from the file specified
         * Note:
         *  This method does not do the above currently. It is hard coded to use the KeywordSimilarityClusterer.
         *  This is a hold out until the proper method is in place to switch between implementations
         *  through a configuration file.
         */
        private bool LoadKeywordClusterer(string filePath)
        {
            KeywordClusterer = new KeywordSimilarityClusterer();

            if (!KeywordClusterer.Load(filePath))
            {
                KeywordClusterer = null;
                return false;
            }
            return true;
        }

        /**
         * Summary:
         *  Loads the the currently selected ProblemPredictor from the file specified
         * Note:
         *  This method does not do the above currently. It is hard coded to use the KNNProblemPredictor.
         *  This is a hold out until the proper method is in place to switch between implementations
         *  through a configuration file.
         */
        private bool LoadProblemPredictor(string filePath)
        {
            ProblemPredictor = new KNNProblemPredictor();
            if(!ProblemPredictor.Load(filePath)) {
                ProblemPredictor = null;
                return false;
            }
            return true;
        }

        private bool RestoreModels()
        {
            if (PartOfSpeechTagger == null)
                return false;
            if (KeywordPredictor == null)
                if (!RestoreKeywordPredictor())
                    return false;
            if (KeywordClusterer == null)
                if (!RestoreKeywordClusterer())
                    return false;
            if (ProblemPredictor == null)
                if (!RestoreProblemPredictor())
                    return false;
            return true;
        }

        private bool RestoreKeywordPredictor()
        {
            var examplesIn = DataLoader.LoadKeywordTrainingExamples(DefaultDataFileLocations.KEYWORD_DATA_TRAINING_FILE);
            var X = KeywordPredictorTrainingUtils.GenerateKeywordTrainingData(examplesIn);
            var Y = KeywordPredictorTrainingUtils.GenerateKeywordTargetData(examplesIn);
            KeywordPredictor.Train(X, Y);
            try
            {
                KeywordPredictor.Save(DefaultModelFileLocations.NAIVE_BAYES_KEYWORD_PREDICTOR_FILE);
            } catch (IOException)
            {
                return false;
            }
            return true;
        }

        private bool RestoreKeywordClusterer()
        {
            List<MechanicQuery> mechanicQueries = DataLoader.LoadMechanicQueries(DefaultDataFileLocations.MECHANIC_QUERY_DATA_FILE);
            List<KeywordExample> trainingExamples = new List<KeywordExample>();
            foreach(MechanicQuery query in mechanicQueries)
            {
                List<List<string>> complaintTags = PartOfSpeechTagger.Tag(
                        SentenceTokenizer.TokenizeSentence(query.Complaint.ToLower())
                );
                List<string> keywords = KeywordPredictor.PredictKeywords(complaintTags);
                KeywordExample example = new KeywordExample();
                foreach (string s in keywords)
                    example.AddKeyword(s);
                trainingExamples.Add(example);
            }
            KeywordClusterer = new KeywordSimilarityClusterer();
            KeywordClusterer.Train(trainingExamples);
            try
            {
                KeywordClusterer.Save(DefaultModelFileLocations.KEYWORD_SIMILARITY_CLUSTERER_FILE);
            } catch (IOException)
            {
                return false;
            }
            return true;
        }

        private bool RestoreProblemPredictor()
        {
            ProblemPredictor = new KNNProblemPredictor();
            List<MechanicQuery> mechanicQueries = DataLoader.LoadMechanicQueries(DefaultDataFileLocations.MECHANIC_QUERY_DATA_FILE);
            List<List<object>> trainingExamples = new List<List<object>>();
            List<object> targetExamples = new List<object>();
            foreach (MechanicQuery query in mechanicQueries)
            {
                List<object> currExample = new List<object>();
                currExample.Add(query.Make);
                currExample.Add(query.Model);
                List<List<string>> complaintTags = PartOfSpeechTagger.Tag(
                        SentenceTokenizer.TokenizeSentence(query.Complaint.ToLower())
                );
                List<string> keywords = KeywordPredictor.PredictKeywords(complaintTags);
                KeywordExample example = new KeywordExample();
                foreach (string s in keywords)
                    example.AddKeyword(s);
                List<int> groupsOut = KeywordClusterer.PredictTopNSimilarGroups(example, NUMBER_COMPLAINT_GROUPS);
                foreach (int i in groupsOut)
                    currExample.Add(i);
                trainingExamples.Add(currExample);
                targetExamples.Add(query.Problem.ToLower());
            }
            ProblemPredictor.Train(trainingExamples, targetExamples);
            try
            {
                ProblemPredictor.Save(DefaultModelFileLocations.KNN_QUERY_PROBLEM_PREDICTOR_FILE);
            } catch(IOException)
            {
                return false;
            }
            return true;
        }
    }
}
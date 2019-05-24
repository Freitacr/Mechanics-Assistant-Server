using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MechanicsAssistantServer.Util;

namespace Mechanic_s_Assistant_Server_Tests.TestUtil
{

    [TestClass]
    public class TestDataLoader
    {
        private static readonly string QueryDataFileName = "tempQueryData.txt";
        private static readonly string KeywordDataFileName = "tempKeywordData.txt";

        private static readonly List<string> Makes = new List<string> { "autocar", "genie", "nissan" };
        private static readonly List<string> Models = new List<string> { "xpeditor", "lift", "semi" };
        private static readonly List<string> Vins = new List<string> { "NAH3", "NOFE", "NOPE" };
        private static readonly List<string> Complaints = new List<string> { "runs rough", "oil leak", "wont start" };
        private static readonly List<string> Problems = new List<string> { "cracked #6 piston", "oil pan cracked", "bad icm" };

        private static readonly List<string> Words = new List<string> { "oil", "pan", "leaks" };
        private static readonly List<string> PartsOfSpeech = new List<string> { "NN", "ADJ", "VBN" };

        private static List<MechanicQuery> GeneratedQueries;
        private static List<KeywordTrainingExample> GeneratedKeywords;

        private static List<MechanicQuery> GenerateQueryData(int numExamples)
        {
            List<MechanicQuery> retQueries = new List<MechanicQuery>();
            Random rand = new Random();
            for (int i = 0; i < numExamples; i++)
            {
                MechanicQuery currQuery = new MechanicQuery(
                    Makes[rand.Next(Makes.Count)],
                    Models[rand.Next(Models.Count)],
                    Vins[rand.Next(Vins.Count)],
                    Complaints[rand.Next(Complaints.Count)],
                    Problems[rand.Next(Problems.Count)]
                );
                retQueries.Add(currQuery);
            }
            GeneratedQueries = retQueries;
            return retQueries;
        }

        private static List<KeywordTrainingExample> GenerateKeywordData(int numExamples)
        {
            List<KeywordTrainingExample> retKeywords = new List<KeywordTrainingExample>();
            Random rand = new Random();
            for (int i = 0; i < numExamples; i++)
            {
                List<KeywordTrainingExample.KeywordPair> currKeywords = new List<KeywordTrainingExample.KeywordPair>();
                for (int j = 0; j < 3; j++)
                {
                    currKeywords.Add(new KeywordTrainingExample.KeywordPair(
                        Words[rand.Next(Words.Count)],
                        PartsOfSpeech[rand.Next(PartsOfSpeech.Count)])
                    );
                }
                KeywordTrainingExample example = new KeywordTrainingExample(
                    currKeywords,
                    rand.Next(2) == 0
                );
                retKeywords.Add(example);
            }
            GeneratedKeywords = retKeywords;
            return retKeywords;
        }

        [ClassInitialize]
        public static void InitDataSource(TestContext ctx)
        {
            StreamWriter queryDataWriter = null;
            StreamWriter keywordDataWriter = null;
            try
            {
                queryDataWriter = new StreamWriter(QueryDataFileName);
                keywordDataWriter = new StreamWriter(KeywordDataFileName);
            } catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("This cannot be recovered from, exiting...");
                System.Environment.Exit(1);
            }
            List<MechanicQuery> data = GenerateQueryData(6);
            var dataSerializer = new DataContractJsonSerializer(typeof(List<MechanicQuery>));
            dataSerializer.WriteObject(queryDataWriter.BaseStream, data);
            queryDataWriter.Close();

            List<KeywordTrainingExample> keywordData = GenerateKeywordData(6);
            dataSerializer = new DataContractJsonSerializer(typeof(List<KeywordTrainingExample>));
            dataSerializer.WriteObject(keywordDataWriter.BaseStream, keywordData);
            keywordDataWriter.Close();
        }

        [ClassCleanup]
        public static void CleanupDataSource()
        {
            File.Delete(QueryDataFileName);
            File.Delete(KeywordDataFileName);
        }

        [TestMethod]
        public void TestLoadKeywordData()
        {
            DataLoader loader = new DataLoader();
            List<KeywordTrainingExample> loadedQueries = loader.LoadKeywordTrainingExamples(KeywordDataFileName);
            Assert.AreEqual(loadedQueries.Count, GeneratedKeywords.Count,
                "The count of the generated and the loaded data are different");
            for (int i = 0; i < loadedQueries.Count; i++)
            {
                Assert.AreEqual(loadedQueries[i], GeneratedKeywords[i],
                    "Two of the queries are different between the generated list and the loaded list");
            }
        }

        [TestMethod]
        public void TestLoadQueryData()
        {
            DataLoader loader = new DataLoader();
            List<MechanicQuery> loadedQueries = loader.LoadMechanicQueries(QueryDataFileName);
            Assert.AreEqual(loadedQueries.Count, GeneratedQueries.Count, 
                "The count of the generated and the loaded data are different");
            for (int i = 0; i < loadedQueries.Count; i++)
            {
                Assert.AreEqual(loadedQueries[i], GeneratedQueries[i],
                    "Two of the queries are different between the generated list and the loaded list");
            }
        }
    }
}

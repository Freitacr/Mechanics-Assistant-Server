using System;
using System.Collections.Generic;
using System.Text;
using OldManinTheShopServer.Models.KeywordClustering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MechanicsAssistantServerTests.TestModels
{
    [TestClass]
    public class TestKeywordSimilarityClusterer
    {

        private List<KeywordExample> Exs1;
        private KeywordSimilarityClusterer Clusterer;
        private readonly string TempFileLoc = "TempFileLoc";

        [TestInitialize]
        public void Init()
        {
            Clusterer = new KeywordSimilarityClusterer();
            GenerateExamples1();
        }

        private void GenerateExamples1()
        {
            Exs1 = new List<KeywordExample>();
            KeywordExample curr = new KeywordExample();
            curr.AddKeyword("oil");
            curr.AddKeyword("pan");
            curr.AddKeyword("leak");
            Exs1.Add(curr);
            curr = new KeywordExample();
            curr.AddKeyword("oil");
            curr.AddKeyword("pan");
            Exs1.Add(curr);
            curr = new KeywordExample();
            curr.AddKeyword("oil");
            curr.AddKeyword("leak");
            curr.AddKeyword("head");
            curr.AddKeyword("gasket");
            Exs1.Add(curr);
            curr = new KeywordExample();
            curr.AddKeyword("fuel");
            curr.AddKeyword("leak");
            curr.AddKeyword("line");
            Exs1.Add(curr);
            curr = new KeywordExample();
            curr.AddKeyword("transmission");
            curr.AddKeyword("fluid");
            curr.AddKeyword("leak");
            Exs1.Add(curr);
        }

        [TestMethod]
        public void TestTrain()
        {
            Clusterer.Train(Exs1);
            //Yes this test is mostly to assert that training doesn't explode. 
        }

        [TestMethod]
        public void TestLoad()
        {
            TestTrain();
            TestSave();
            KeywordSimilarityClusterer clusterer2 = new KeywordSimilarityClusterer();
            var reader = new System.IO.StreamReader(TempFileLoc);
            clusterer2.Load(reader.BaseStream);
            reader.Close();
            KeywordExample test = new KeywordExample();
            test.AddKeyword("leak");
            Assert.AreEqual(clusterer2.PredictGroupSimilarity(test)[0], Clusterer.PredictGroupSimilarity(test)[0]);
        }

        [TestMethod]
        public void TestSave()
        {
            TestTrain();
            var writer = new System.IO.StreamWriter(TempFileLoc);
            Clusterer.Save(writer.BaseStream);
        }

        [TestMethod]
        public void TestCalculateGroupSimilarity()
        {
            TestTrain();
            KeywordExample test = new KeywordExample();
            test.AddKeyword("leak");
            var list = Clusterer.PredictGroupSimilarity(test);
            Assert.AreEqual(list[0], 1);
            Assert.AreEqual(list[1], 2);
        }

        [TestMethod]
        public void TestCalculateTopNGroupSimiliarity()
        {
            TestTrain();
            KeywordExample test = new KeywordExample();
            test.AddKeyword("leak");
            var list = Clusterer.PredictTopNSimilarGroups(test, 1);
            Assert.AreEqual(list[0], 1);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using OldManinTheShopServer.Models.KeywordClustering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MechanicsAssistantServerTests.TestModels.TestKeywordClustering
{
    [TestClass]
    public class TestKeywordGroup
    {

        private KeywordGroup G1, G2, G3;
        private List<ClaimableKeywordExample> Exs1, Exs2;
        private KeywordExample Ex1, Ex2;

        [TestInitialize]
        public void InitTest()
        {
            GenerateKeywordExamples();
            GenerateGroups();
        }

        private void GenerateKeywordExamples()
        {
            Exs1 = new List<ClaimableKeywordExample>();
            Exs2 = new List<ClaimableKeywordExample>();
            GenerateExamples1();
            GenerateExamples2();
            Ex1 = new KeywordExample();
            Ex1.AddKeyword("transmission");
            Ex1.AddKeyword("fluid");
            Ex1.AddKeyword("leak");
            Ex2 = new KeywordExample();
            Ex2.AddKeyword("head");
            Ex2.AddKeyword("gasket");
            Ex2.AddKeyword("blown");
        }

        private void GenerateExamples1()
        {
            KeywordExample curr = new KeywordExample();
            curr.AddKeyword("oil");
            curr.AddKeyword("pan");
            curr.AddKeyword("leak");
            Exs1.Add(new ClaimableKeywordExample(curr));
            curr = new KeywordExample();
            curr.AddKeyword("oil");
            curr.AddKeyword("pan");
            Exs1.Add(new ClaimableKeywordExample(curr));
            curr = new KeywordExample();
            curr.AddKeyword("oil");
            curr.AddKeyword("leak");
            curr.AddKeyword("head");
            curr.AddKeyword("gasket");
            Exs1.Add(new ClaimableKeywordExample(curr));
            curr = new KeywordExample();
            curr.AddKeyword("fuel");
            curr.AddKeyword("leak");
            curr.AddKeyword("line");
            Exs1.Add(new ClaimableKeywordExample(curr));
            curr = new KeywordExample();
            curr.AddKeyword("transmission");
            curr.AddKeyword("fluid");
            curr.AddKeyword("leak");
            Exs1.Add(new ClaimableKeywordExample(curr));
        }

        private void GenerateExamples2()
        {
            KeywordExample curr = new KeywordExample();
            curr.AddKeyword("icm");
            curr.AddKeyword("malfunction");
            Exs2.Add(new ClaimableKeywordExample(curr));
            curr = new KeywordExample();
            curr.AddKeyword("starter");
            curr.AddKeyword("engaging");
            Exs2.Add(new ClaimableKeywordExample(curr));
            curr = new KeywordExample();
            curr.AddKeyword("plugged");
            curr.AddKeyword("dpf");
            Exs2.Add(new ClaimableKeywordExample(curr));
            curr = new KeywordExample();
            curr.AddKeyword("fuel");
            curr.AddKeyword("leak");
            curr.AddKeyword("line");
            Exs2.Add(new ClaimableKeywordExample(curr));
            curr = new KeywordExample();
            curr.AddKeyword("transmission");
            curr.AddKeyword("seized");
            Exs2.Add(new ClaimableKeywordExample(curr));
        }

        private void GenerateGroups()
        {
            G1 = new KeywordGroup("oil");
            G1.SelectedKeywords.AddKeyword("pan");
            G2 = new KeywordGroup("transmission");
            G3 = new KeywordGroup("oil");
            G3.SelectedKeywords.AddKeyword("leak");
        }

        [TestMethod]
        public void TestGenerateSubGroups()
        {
            KeywordGroup g = new KeywordGroup("oil");
            g.UpdateMembers(Exs1);
            var groups = g.GenerateSubGroups(2, 1);
            Assert.AreEqual(groups.Count, 9);

            g = new KeywordGroup("leak");
            var tempex = new ClaimableKeywordExample(new KeywordExample());
            tempex.ContainedExample.AddKeyword("exhaust");
            tempex.ContainedExample.AddKeyword("leak");
            Exs1.Add(tempex);
            g.UpdateMembers(Exs1);
            groups = g.GenerateSubGroups(2, 1);
            Assert.AreEqual(groups.Count, 5);
            var tempGroup = new KeywordGroup("leak");
            tempGroup.SelectedKeywords.AddKeyword("oil");
            Assert.IsTrue(groups.Contains(tempGroup));
        }

        [TestMethod]
        public void TestUpdateMembers()
        {
            G1.UpdateMembers(Exs1);
            Assert.AreEqual(G1.Count, 2);
            G2.UpdateMembers(Exs2);
            Assert.AreEqual(G2.Count, 1);
            G3.UpdateMembers(Exs2);
            Assert.AreEqual(G3.Count, 0);
        }

        [TestMethod]
        public void TestCalculateSimilarityScoreGroup()
        {
            G1.UpdateMembers(Exs1);
            G3.UpdateMembers(Exs1);
            Assert.AreEqual(1 / (double)2, G1.CalculateSimilarityScore(G3), .1);
        }

        [TestMethod]
        public void TestCalculateSimilarityScoreExample()
        {
            Assert.AreEqual(G1.CalculateSimilarityScore(Ex2), 0);
            Assert.AreEqual(G2.CalculateSimilarityScore(Ex2), 0);
            Assert.AreEqual(G3.CalculateSimilarityScore(Ex2), 0);
            Assert.AreEqual(1 / (double)3, G3.CalculateSimilarityScore(Ex1), .1);
            Assert.AreEqual(1 / (double)3, G2.CalculateSimilarityScore(Ex1), .1);
            Assert.AreEqual(0, G1.CalculateSimilarityScore(Ex1), .1);
        }


    }
}

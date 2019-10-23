using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldManinTheShopServer.Models.KeywordClustering;

namespace MechanicsAssistantServerTests.TestModels.TestKeywordClustering
{
    [TestClass]
    public class TestKeywordExample
    {

        private KeywordExample Ex1, Ex2, Ex3, Ex4;

        [TestInitialize]
        public void InitTests()
        {
            Ex1 = new KeywordExample();
            Ex2 = new KeywordExample();
            Ex3 = new KeywordExample();
            Ex4 = new KeywordExample();
            Ex1.AddKeyword("a");
            Ex1.AddKeyword("b");
            Ex1.AddKeyword("c");
            Ex2.AddKeyword("c");
            Ex2.AddKeyword("A");
            Ex2.AddKeyword("B");
            Ex3.AddKeyword("B");
        }

        [TestMethod]
        public void TestCountSimiliarVsEmpty()
        {
            Assert.AreEqual(Ex1.CountSimilar(Ex4), 0);
            Assert.AreEqual(Ex2.CountSimilar(Ex4), 0);
            Assert.AreEqual(Ex3.CountSimilar(Ex4), 0);
            Assert.AreEqual(Ex4.CountSimilar(Ex4), 0);
        }

        [TestMethod]
        public void TestCountSimilarVsSelf()
        {
            Assert.AreEqual(Ex1.CountSimilar(Ex1), Ex1.Count);
            Assert.AreEqual(Ex2.CountSimilar(Ex2), Ex2.Count);
            Assert.AreEqual(Ex3.CountSimilar(Ex3), Ex3.Count);
        }

        [TestMethod]
        public void TestCountSimilarCasewise()
        {
            Assert.AreEqual(Ex1.CountSimilar(Ex2), 3);
            Assert.AreEqual(Ex2.CountSimilar(Ex3), 1);
            Assert.AreEqual(Ex1.CountSimilar(Ex3), 1);
        }
    }
}

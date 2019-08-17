using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MechanicsAssistantServer.Models;
using System.IO;

namespace Mechanic_s_Assistant_Server_Tests.TestModels
{
    [TestClass]
    public class TestKNN
    {
        private List<string> StringList = new List<string> { "acbe", "bde", "acc", "ied" };

        private KNN KnnModel;

        private double CalcDistance(List<double> pointA, List<double> pointB)
        {
            double distance = 0;
            if (pointA.Count != pointB.Count)
                Assert.Fail("Point A and B had a different number of elements");
            for (int i = 0; i < pointA.Count; i++)
                distance += Math.Pow((pointA[i] - pointB[i]), 2);
            return Math.Sqrt(distance);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            KnnModel = new KNN();
        }
        
        private List<object> GenerateValidList(int numElements)
        {
            List<object> retList = new List<object>();
            Random rand = new Random();
            retList.Add(StringList[rand.Next(StringList.Count)]);
            retList.Add(StringList[rand.Next(StringList.Count)]);
            for (int i = 2; i < numElements; i++)
            {
                retList.Add(rand.NextDouble());
            }
            return retList;
        }

        private List<object> GenerateValidTargetData(int numElements)
        {
            List<object> ret = new List<object>();
            Random rand = new Random();
            for (int i = 0; i < numElements; i++)
            {
                ret.Add(rand.Next(StringList.Count));
            }
            return ret;
        }

        private List<object> GenerateInvalidList(int numElements)
        {
            List<object> ret = new List<object>();
            Random rand = new Random();
            for (int i = 0; i < numElements; i++)
            {
                if (i % 2 == 0)
                    ret.Add((char)rand.Next(256));
                else 
                    ret.Add(rand.NextDouble());
            }
            return ret;
        }

        /**
         * The most we can do here is simply check that no errors were thrown
         * during the process of training
         */
        [TestMethod]
        public void TestTrainValidData()
        {
            List<List<object>> validData = new List<List<object>>();
            for (int i = 0; i < 5; i++)
            {
                validData.Add(GenerateValidList(5));
            }
            List<object> validTargetData = GenerateValidTargetData(5);
            KnnModel.Train(validData, validTargetData);
        }

        /**
         * Test that the KNN does not attempt to allow data through that
         * Does not have all of the data elements being the same length
         */
        [TestMethod]
        public void TestTrainDataIncorrectLengths()
        {
            List<List<object>> invalidData = new List<List<object>>();
            List<object> validTargetData = GenerateValidTargetData(5);
            int previousCount = -1;
            Random rand = new Random();
            for (int i = 0; i < 5; i++)
            {
                int genCount = rand.Next(5) + 1;
                while (genCount == previousCount)
                    genCount = rand.Next(5) + 1;
                invalidData.Add(GenerateValidList(genCount));
                previousCount = genCount;
            }
            
            try
            {
                KnnModel.Train(invalidData, validTargetData);
                Assert.Fail("Method should have thrown an exception, but trained successfully.");
            } catch(InvalidDataFormatException)
            {
                //Expected, ignore
            }
        }

        /**
         * <summary>Recalls the <c>TestTrainValidData</c> method to set up a KNN model, then saves
         * and loads it from a file to ensure that saving and loading are working.</summary>
         * 
         */
        [TestMethod]
        public void TestSaveAndLoadKNNModel()
        {
            TestTrainValidData(); //Sets up a KNN with data
            string modelFileName = "KNNModel.txt";
            KnnModel.Save(modelFileName);
            List<Dictionary<object, int>> labelMappingDict = KnnModel.CopyLabelMappingDictionary();
            List<KNNDataPoint> dataPoints = KnnModel.CopyDataPoints();
            KnnModel = new KNN();
            KnnModel.Load(modelFileName);
            List<KNNDataPoint> loadedDataPoints = KnnModel.CopyDataPoints();
            var loadedMappingDicts = KnnModel.CopyLabelMappingDictionary();
            for (int i = 0; i < dataPoints.Count; i++)
                Assert.IsTrue(dataPoints[i].Equals(loadedDataPoints[i]));
            for (int i = 0; i < labelMappingDict.Count; i++)
            {
                var savedMappingDict = labelMappingDict[i];
                var loadedMappingDict = loadedMappingDicts[i];
                foreach(object key in savedMappingDict.Keys)
                    Assert.IsTrue(savedMappingDict[key].Equals(loadedMappingDict[key]));
            }
            File.Delete(modelFileName);
        }

        /**
         * Data is deemed invalid for training  in a strange way:
         * Consider this data element: ['a', 'b', 0, 1, 2.5]
         * If that data element is at the beginning of the list,
         * Then ALL other data elements MUST have non-numeric objects
         * In the first two elements of the list, and 3 numeric elements
         * in the last three elements of the list. Otherwise, the data is invalid.
         */
        [TestMethod]
        public void TestTrainDataInvalid()
        {
            List<List<object>> invalidData = new List<List<object>>();
            List<object> validTargetData = GenerateValidTargetData(5);
            for (int i = 0; i < 5; i++)
            {
                if (i % 2 == 0)
                    invalidData.Add(GenerateInvalidList(5));
                else invalidData.Add(GenerateValidList(5));
            }

            try
            {
                KnnModel.Train(invalidData, validTargetData);
                Assert.Fail("Method should have thrown an exception, but trained successfully.");
            }
            catch (InvalidDataFormatException)
            {
                //Expected, ignore
            }
        }

        /**
         * Another instance in which the data could be deemed invalid:
         * Where the training and target data are of different lengths
         */
        [TestMethod]
        public void TestTrainTargetDataInvalidLength()
        {
            List<List<object>> validData = new List<List<object>>();
            for (int i = 0; i < 5; i++)
            {
                validData.Add(GenerateValidList(5));
            }
            List<object> validTargetData = GenerateValidTargetData(6);
            try
            {
                KnnModel.Train(validData, validTargetData);
                Assert.Fail("Method should have thrown an exception, but trained successfully.");
            }
            catch (InvalidDataFormatException)
            {
                //Expected, ignore
            }
        }

        [TestMethod]
        public void TestPredictRunsSuccessfullyWithoutException()
        {
            TestTrainValidData();
            List<object> validData = GenerateValidList(5);
            KnnModel.Predict(validData, CalcDistance);
            KnnModel.PredictTopN(validData, CalcDistance, 6);
        }

    }
}

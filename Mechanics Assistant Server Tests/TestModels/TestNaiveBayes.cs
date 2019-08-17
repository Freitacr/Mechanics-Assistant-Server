using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MechanicsAssistantServer.Models;

namespace Mechanic_s_Assistant_Server_Tests.TestModels
{
    [TestClass]
    public class TestNaiveBayes
    {

        private readonly List<List<object>> PreDeterminedX = new List<List<object>>
        {
            new List<object> {"Sunny" },
            new List<object> {"Sunny" },
            new List<object> {"Sunny" },
            new List<object> {"Sunny" },
            new List<object> {"Sunny" },
            new List<object> {"Overcast" },
            new List<object> {"Overcast" },
            new List<object> {"Overcast" },
            new List<object> {"Overcast" },
            new List<object> {"Rainy" },
            new List<object> {"Rainy" },
            new List<object> {"Rainy" },
            new List<object> {"Rainy" },
            new List<object> {"Rainy" }
        };
        private readonly List<object> PreDeterminedY = new List<object>
        {
            "Yes",
            "Yes",
            "Yes",
            "No",
            "No",
            "Yes",
            "Yes",
            "Yes",
            "Yes",
            "Yes",
            "Yes",
            "No",
            "No",
            "No"
        };

        private NaiveBayes NaiveBayesModel;

        [TestInitialize]
        public void Setup()
        {
            NaiveBayesModel = new NaiveBayes();
        }

        private readonly List<string> PossibleEntries = new List<string> { "hi", "by", "try", "cry" };

        private List<object> GenerateDecimalInclusiveData(int numElements)
        {
            List<object> ret = new List<object>();
            Random rand = new Random();
            bool hasDec = false;
            for (int i = 0; i < numElements; i++)
            {
                if (rand.Next(1, 6) == 1)
                {
                    hasDec = true;
                    ret.Add(rand.NextDouble());
                }
                else
                {
                    if (rand.Next(2) == 0)
                        ret.Add(rand.Next(100));
                    else
                        ret.Add(PossibleEntries[rand.Next(PossibleEntries.Count)]);
                }
            }
            if (!hasDec)
            {
                int selectedIndex = rand.Next(numElements);
                ret[selectedIndex] = rand.NextDouble();
            }
            return ret;
        }

        private List<object> GenerateValidData(int numElements)
        {
            List<object> ret = new List<object>();
            Random rand = new Random();
            for (int i = 0; i < numElements; i++)
            {
                if (rand.Next(2) == 0)
                    ret.Add(rand.Next(100));
                else
                    ret.Add(PossibleEntries[rand.Next(PossibleEntries.Count)]);
            }
            return ret;
        }

        /**
         * <summary>This test should produce an <c>InvalidDataFormatException</c> as
         * a requirement of data passed to the training (and later the prediction) method
         * are required to be of the same length. </summary>
         * <example>
         * So, for a training data set X, with a target data set Y,
         * Y must be the same length as X
         * and all elements of X must contain the same number of elements
         * </example>
         */
        [TestMethod]
        public void TestTrainInconsistentTrainingDataLength()
        {
            int numTrainingExamples = 6;
            int numDataElements = 5;
            List<List<object>> trainingData = new List<List<object>>();
            List<object> targetData = GenerateValidData(numTrainingExamples);
            Random rand = new Random();
            bool hasInconsistentData = false;
            for (int i = 0; i < numTrainingExamples; i++)
            {
                if (rand.Next(2) == 0)
                {
                    trainingData.Add(GenerateValidData(rand.Next(numTrainingExamples)));
                    hasInconsistentData = true;
                }
                else
                {
                    trainingData.Add(GenerateValidData(numDataElements));
                }
            }
            if (!hasInconsistentData)
            {
                trainingData[rand.Next(numTrainingExamples)] = GenerateValidData(rand.Next(numTrainingExamples, 1000));
            }
            try
            {
                NaiveBayesModel.Train(trainingData, targetData);
                Assert.Fail("Model was required to throw an InvalidDataFormatException, but did not");
            }
            catch (InvalidDataFormatException)
            {
                //All ok...
            }
        }

        [TestMethod]
        public void TestTrainInconsistentTrainingAndTargetDataLengths()
        {
            int numTrainingExamples = 6;
            int numDataElements = 5;
            List<List<object>> trainingData = new List<List<object>>();
            Random rand = new Random();
            List<object> shorterTargetData = GenerateValidData(numTrainingExamples - rand.Next(1, 5));
            List<object> longerTargetData = GenerateValidData(numTrainingExamples + rand.Next(1, 5));
            bool hasInconsistentData = false;
            for (int i = 0; i < numTrainingExamples; i++)
            {
                if (rand.Next(2) == 0)
                {
                    trainingData.Add(GenerateValidData(rand.Next(numTrainingExamples)));
                    hasInconsistentData = true;
                }
                else
                {
                    trainingData.Add(GenerateValidData(numDataElements));
                }
            }
            if (!hasInconsistentData)
            {
                trainingData[rand.Next(numTrainingExamples)] = GenerateValidData(rand.Next(numTrainingExamples, 1000));
            }
            try
            {
                NaiveBayesModel.Train(trainingData, shorterTargetData);
                Assert.Fail("Model was required to throw an InvalidDataFormatException, but did not");
            }
            catch (InvalidDataFormatException)
            {
                //All ok...
            }

            try
            {
                NaiveBayesModel.Train(trainingData, longerTargetData);
                Assert.Fail("Model was required to throw an InvalidDataFormatException, but did not");
            } catch (InvalidDataFormatException)
            {
                //All ok...
            }
        }

        /**
         * <summary>This should produce an InvalidDataFormatException as floating point numbers
         * cannot be handled well by this implementation of the Niave Bayes Algorithm</summary>
         */
         /*
        [TestMethod]
        public void TestTrainIncludingDecimals()
        {
            int numTrainingExamples = 6;
            int numDataElements = 5;
            List<List<object>> trainingData = new List<List<object>>();
            List<object> targetData = GenerateValidData(numTrainingExamples);
            Random rand = new Random();
            bool hasFloatingPointData = false;
            for (int i = 0; i < numTrainingExamples; i++)
            {
                if (rand.Next(2) == 0)
                {
                    trainingData.Add(GenerateDecimalInclusiveData(numDataElements));
                    hasFloatingPointData = true;
                } else
                {
                    trainingData.Add(GenerateValidData(numDataElements));
                }
            }
            if (!hasFloatingPointData)
            {
                trainingData[rand.Next(numTrainingExamples)] = GenerateDecimalInclusiveData(numDataElements);
            }
            try
            {
                NaiveBayesModel.Train(trainingData, targetData);
                Assert.Fail("Model was required to throw an InvalidDataFormatException, but did not");
            } catch (InvalidDataFormatException)
            {
                //All ok...
            }
        }
        */


        /**
         * <summary>This test is simply to confirm that training does not cause an exception
         * And that all things run smoothly. It does not confirm the integrity of the model
         * after training however</summary>
         */
        [TestMethod]
        public void TestTrainValidData()
        {
            NaiveBayesModel.Train(PreDeterminedX, PreDeterminedY);
        }

        /**
         * <summary>This test ensures that when a model is saved to a file, and then loaded
         * the model that is loaded will be functionally identical to the model that was
         * saved</summary>
         */ 
        [TestMethod]
        public void TestModelSaveAndLoad()
        {
            TestTrainValidData();
            string modelFileName = "NaiveBayesFile.txt";
            NaiveBayesModel.Save(modelFileName);
            var nbTestModel = new NaiveBayes();
            nbTestModel.Load(modelFileName);
            Assert.AreEqual(nbTestModel, NaiveBayesModel);
            File.Delete(modelFileName);
        }

        [TestMethod]
        public void TestPredictMethod()
        {
            TestTrainValidData();
            object predicted = NaiveBayesModel.Predict(new List<object> { "Sunny" });
            string predictedRes = predicted as string;
            Assert.AreEqual(predictedRes, "Yes");
        }
    }
}

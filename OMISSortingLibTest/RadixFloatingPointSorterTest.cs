using NUnit.Framework;
using OMISSortingLib;
using System;
using System.Collections.Generic;

namespace OMISSortingLibTest
{
    public class Tests
    {
        public float[] ToSort;
        public double[] ToSort2;
        [SetUp]
        public void Setup()
        {
            Random random = new Random();
            List<float> toSort = new List<float>();
            List<double> toSort2 = new List<double>();
            double max = 20.0;
            for (int i = 0; i < 100000; i++)
            {
                toSort.Add((float)(random.NextDouble() * max));
                toSort2.Add(random.NextDouble() * max);
            }
            ToSort = toSort.ToArray();
            ToSort2 = toSort2.ToArray();
        }

        [Test]
        public void TestFloatSort()
        {
            ToSort.RadixSort();
            for(int i = 0; i < ToSort.Length-1; i++)
            {
                Assert.LessOrEqual(ToSort[i], ToSort[i + 1]);
            }
            Assert.Pass();
        }

        [Test]
        public void TestDoubleSort()
        {
            ToSort2.RadixSort();
            for (int i = 0; i < ToSort2.Length - 1; i++)
            {
                Assert.LessOrEqual(ToSort2[i], ToSort2[i + 1]);
            }
            Assert.Pass();
        }
    }
}
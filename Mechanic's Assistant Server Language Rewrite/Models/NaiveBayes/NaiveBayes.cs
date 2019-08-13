using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace MechanicsAssistantServer.Models
{
    [DataContract]
    class ProbabilityStorage
    {
        [DataMember]
        public double Probability { get; set; }
        [DataMember]
        public object Label { get; set; }

        public ProbabilityStorage(double probability, object label)
        {
            Probability = probability;
            Label = label;
        }

        public static int Sort(ProbabilityStorage a, ProbabilityStorage b)
        {
            if (a.Probability < b.Probability)
                return 1;
            if (a.Probability > b.Probability)
                return -1;
            return 0;
        }
    }

    [DataContract]
    public class NaiveBayes
    {
        [DataMember]
        private Dictionary<object, int> ClassMappingDictionary;
        [DataMember]
        private List<Dictionary<object, int>> FeatureMappingDictionaries;
        [DataMember]
        public List<NaiveBayesProbabilityTable> FeatureProbabilityTables;
        [DataMember]
        private List<ProbabilityTableEntry> ClassProbabilityTable;

        public NaiveBayes()
        {
            ClassMappingDictionary = new Dictionary<object, int>();
            FeatureMappingDictionaries = new List<Dictionary<object, int>>();
            FeatureProbabilityTables = new List<NaiveBayesProbabilityTable>();
            ClassProbabilityTable = new List<ProbabilityTableEntry>();
        }

        public void Train(List<List<object>> X, List<object> Y)
        {
            ValidateData(X, Y);
            SetupMappingDictionaries(X, Y);
            SetupProbabilityTables();
            FillProbabilityTables(X, Y);
        }

        public object Predict(List<object> x)
        {
            return PredictTopN(x, 1)[0];
        }

        public List<object> PredictTopN(List<object> x, int n)
        {
            var predictedProbabilities = PredictProbabilities(x);
            var retObjects = new List<object>();
            if (n >= predictedProbabilities.Count)
                foreach (ProbabilityStorage predictedObject in predictedProbabilities)
                    retObjects.Add(predictedObject.Label);
            else
                for (int i = 0; i < n; i++)
                    retObjects.Add(predictedProbabilities[i].Label);
            return retObjects;
        }

        private double CalculateFeatureProbability(NaiveBayesProbabilityTable table, int featureIndex)
        {
            int totalNumerator = 0;
            for (int i = 0; i < ClassMappingDictionary.Count; i++)
            {
                totalNumerator += table[featureIndex][i].Numerator;
            }
            return totalNumerator / (double)ClassProbabilityTable[0].Denominator;
        }

        private double CalculateFeatureLikelihood(NaiveBayesProbabilityTable table, int featureNumerator, int labelIndex)
        {
            int totalDenominator = 0;
            for (int i = 0; i < table.Rows; i++)
                totalDenominator += table[i][labelIndex].Numerator;
            return featureNumerator / (double)totalDenominator;
        }

        private List<ProbabilityStorage> PredictProbabilities(List<object> x)
        {
            var featureIndices = new List<int>();
            for (int i = 0; i < x.Count; i++)
            {
                var featureMappingDict = FeatureMappingDictionaries[i];
                int index = featureMappingDict.GetValueOrDefault(x[i], -1);
                featureIndices.Add(index);
            }
            var retPredictionProbabilites = new List<ProbabilityStorage>();
            foreach (KeyValuePair<object, int> mappedLabel in ClassMappingDictionary)
            {
                double calculatedProbability = 1.0;
                int labelIndex = mappedLabel.Value;
                ProbabilityTableEntry labelEntry = ClassProbabilityTable[labelIndex];
                double labelProbability = labelEntry.Numerator / (double)labelEntry.Denominator;
                for (int i = 0; i < featureIndices.Count; i++)
                {
                    if (featureIndices[i] == -1)
                    {
                        double labelProbabilityPrime = labelEntry.Numerator + 1 / (double)(labelEntry.Denominator + 1);
                        throw new NotImplementedException("New feature mapping has not been implemented as of yet");
                    }
                    else
                    {
                        int featureIndex = featureIndices[i];
                        NaiveBayesProbabilityTable currTable = FeatureProbabilityTables[i];
                        ProbabilityTableEntry entry = FeatureProbabilityTables[i][featureIndex][labelIndex];
                        double featureProbability = CalculateFeatureProbability(currTable, featureIndex);
                        double divisionResult = CalculateFeatureLikelihood(currTable, entry.Numerator, labelIndex);
                        if (divisionResult == 0)
                            throw new NotImplementedException("New Case mapping has not been implemented as of yet");

                        calculatedProbability *= (divisionResult * labelProbability) / featureProbability;
                    }
                }
                retPredictionProbabilites.Add(new ProbabilityStorage(calculatedProbability, mappedLabel.Key));
            }
            retPredictionProbabilites.Sort(ProbabilityStorage.Sort);
            return retPredictionProbabilites;
        }

        public void Load(string filePath)
        {
            StreamReader fileReader = new StreamReader(filePath);
            DataContractJsonSerializer naiveBayesSerializer = new DataContractJsonSerializer(this.GetType());
            var naiveBayesObject = (NaiveBayes)naiveBayesSerializer.ReadObject(fileReader.BaseStream);
            fileReader.Close();
            this.FeatureMappingDictionaries = naiveBayesObject.FeatureMappingDictionaries;
            this.ClassMappingDictionary = naiveBayesObject.ClassMappingDictionary;
            this.FeatureProbabilityTables = naiveBayesObject.FeatureProbabilityTables;
            this.ClassProbabilityTable = naiveBayesObject.ClassProbabilityTable;
        }

        public void Save(string filePath)
        {
            StreamWriter fileWriter = new StreamWriter(filePath);
            DataContractJsonSerializer selfSerializer = new DataContractJsonSerializer(this.GetType());
            selfSerializer.WriteObject(fileWriter.BaseStream, this);
            fileWriter.Close();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            NaiveBayes other = obj as NaiveBayes;
            return NaiveBayesEqualityHelper.AreEqual(other, this);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private void ValidateData(List<List<object>> X, List<object> Y)
        {
            if (X.Count == 0)
                throw new InvalidDataFormatException("Training data was empty!");
            if (Y.Count == 0)
                throw new InvalidDataFormatException("Target data was empty!");
            int trainingDataLength = X[0].Count;
            for (int i = 1; i < X.Count; i++)
                if (trainingDataLength != X[i].Count)
                    throw new InvalidDataFormatException("Some training examples have different amounts of features!");
            if (Y.Count != X.Count)
                throw new InvalidDataFormatException("Training and Target data have different numbers of examples!");
        }

        private void SetupClassProbabilityTable()
        {
            for (int i = 0; i < ClassMappingDictionary.Count; i++)
                ClassProbabilityTable.Add(new ProbabilityTableEntry());
        }

        private void SetupFeatureProbabilityTables()
        {
            for (int i = 0; i < FeatureMappingDictionaries.Count; i++) {
                var probabilityTable = new NaiveBayesProbabilityTable();
                for (int j = 0; j < FeatureMappingDictionaries[i].Count; j++)
                    probabilityTable.AddRow(ClassMappingDictionary.Count);
                FeatureProbabilityTables.Add(probabilityTable);
            }
        }

        private void SetupProbabilityTables()
        {
            SetupClassProbabilityTable();
            SetupFeatureProbabilityTables();
        }

        private void SetupMappingDictionary(List<object> objectsIn, Dictionary<object, int> dict)
        {
            foreach (object label in objectsIn)
            {
                if (!dict.ContainsKey(label))
                {
                    dict[label] = dict.Count;
                }
            }
        }

        private void SetupClassMappingDictionary(List<object> Y)
        {
            SetupMappingDictionary(Y, ClassMappingDictionary);
        }

        private void SetupFeatureMappingDictionaries(List<List<object>> X)
        {
            for (int i = 0; i < X[0].Count; i++)
            {
                var newMappingDict = new Dictionary<object, int>();
                foreach (List<object> dataList in X)
                {
                    if (!newMappingDict.ContainsKey(dataList[i]))
                    {
                        newMappingDict[dataList[i]] = newMappingDict.Count;
                    }
                }
                FeatureMappingDictionaries.Add(newMappingDict);
            }
        }

        private void SetupMappingDictionaries(List<List<object>> X, List<object> Y)
        {
            ClassMappingDictionary = new Dictionary<object, int>();
            FeatureMappingDictionaries = new List<Dictionary<object, int>>();
            SetupClassMappingDictionary(Y);
            SetupFeatureMappingDictionaries(X);
        }

        private void FillProbabilityTables(List<List<object>> X, List<object> Y)
        {
            for (int i = 0; i < X.Count; i++)
            {
                int labelIndex = ClassMappingDictionary[Y[i]];
                ClassProbabilityTable[labelIndex].Numerator++;
                
                for (int featureIndex = 0; featureIndex < X[i].Count; featureIndex++)
                {
                    var currDict = FeatureMappingDictionaries[featureIndex];
                    var featureTable = FeatureProbabilityTables[featureIndex];
                    object currFeature = X[i][featureIndex];
                    int mappedFeatureIndex = currDict[currFeature];
                    featureTable[mappedFeatureIndex][labelIndex].Numerator++;
                }
            }
            foreach (NaiveBayesProbabilityTable featureTable in FeatureProbabilityTables)
                featureTable.DetermineDenominator();
            int count = 0;
            foreach (ProbabilityTableEntry entry in ClassProbabilityTable)
                count += entry.Numerator;
            foreach (ProbabilityTableEntry entry in ClassProbabilityTable)
                entry.Denominator = count;
        }

        public List<Dictionary<object, int>> GetFeatureMappingDictionaries()
        {
            return FeatureMappingDictionaries;
        }

        public Dictionary<object, int> GetClassMappingDictionary()
        {
            return ClassMappingDictionary;
        }

        public List<ProbabilityTableEntry> GetClassProbabilityTable()
        {
            return ClassProbabilityTable;
        }

        public List<NaiveBayesProbabilityTable> GetFeatureProbabilityTables()
        {
            return FeatureProbabilityTables;
        }
    }


    class NaiveBayesEqualityHelper
    {

        private static bool AreDictionariesEqual(Dictionary<object, int> a, Dictionary<object, int> b)
        {
            if (a.Count != b.Count)
                return false;
            foreach (KeyValuePair<object, int> containedPair in a)
            {
                if (b[containedPair.Key] != containedPair.Value)
                    return false;
            }
            return true;
        }

        public static bool AreEqual(NaiveBayes a, NaiveBayes b)
        {
            if (!AreDictionariesEqual(a.GetClassMappingDictionary(), b.GetClassMappingDictionary()))
                return false;

            var aFeatureMappingDictionaries = a.GetFeatureMappingDictionaries();
            var bFeatureMappingDictionaries = b.GetFeatureMappingDictionaries();
            if (aFeatureMappingDictionaries.Count != bFeatureMappingDictionaries.Count)
                return false;
            for (int i = 0; i < aFeatureMappingDictionaries.Count; i++)
                if (!AreDictionariesEqual(aFeatureMappingDictionaries[i], bFeatureMappingDictionaries[i]))
                    return false;
            var aClassProbabilityTable = a.GetClassProbabilityTable();
            var bClassProbabilityTable = b.GetClassProbabilityTable();
            if (aClassProbabilityTable.Count != bClassProbabilityTable.Count)
                return false;
            for (int i = 0; i < aClassProbabilityTable.Count; i++)
                if (!aClassProbabilityTable[i].Equals(bClassProbabilityTable[i]))
                    return false;
            var aFeatureProbabilityTables = a.GetFeatureProbabilityTables();
            var bFeatureProbabilityTables = b.GetFeatureProbabilityTables();
            if (aFeatureProbabilityTables.Count != bFeatureProbabilityTables.Count)
                return false;
            for (int i = 0; i < aFeatureProbabilityTables.Count; i++)
                if (!aFeatureProbabilityTables[i].Equals(bFeatureProbabilityTables[i]))
                    return false;
            return true;
        }
    }
}

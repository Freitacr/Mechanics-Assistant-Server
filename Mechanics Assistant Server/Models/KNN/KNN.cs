using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MechanicsAssistantServer.Models
{
    [DataContract]
    public class KNNDataPoint
    {
        [DataMember]
        public List<double> DataPoints { get; set; }

        [DataMember]
        public object ExpectedResult { get; set; }

        public KNNDataPoint(List<double> dataPoints, object expectedResult)
        {
            DataPoints = dataPoints;
            ExpectedResult = expectedResult;
        }


        // override object.Equals
        public override bool Equals(object obj)
        { 

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            KNNDataPoint other = obj as KNNDataPoint;
            if (DataPoints.Count != other.DataPoints.Count)
                return false;
            for (int i = 0; i < DataPoints.Count; i++)
                if (DataPoints[i] != other.DataPoints[i])
                    return false;
            return ExpectedResult.Equals(other.ExpectedResult);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            return base.GetHashCode();
        }
    }

    internal static class KNNTypeDictionaries
    {
        public static Dictionary<Type, bool> USE_RAW { get; private set; } = null;
        public static void InitializeTypeDictionaries()
        {
            if (USE_RAW == null)
            {
                USE_RAW = new Dictionary<Type, bool>();
                USE_RAW[typeof(double)] = true;
                USE_RAW[typeof(int)] = true;
                USE_RAW[typeof(short)] = true;
                USE_RAW[typeof(float)] = true;
            }
        }
    }

    /**
     * <summary><c>KNN</c> is a modified implementation of the K Nearest Neighbours algorithm</summary>  
     * It is modified in the sense that it returns the classes of the top K most similar data points
     * <author>Colton Freitas</author>
     */
    public class KNN
    {
        private List<Dictionary<object, int>> LabelMappingDictionaries;

        private List<KNNDataPoint> DataPoints;

        public KNN()
        {
            LabelMappingDictionaries = new List<Dictionary<object, int>>();
            DataPoints = new List<KNNDataPoint>();
            KNNTypeDictionaries.InitializeTypeDictionaries();
        }

        private Type TypeOfDataElement(object obj)
        {
            Type ret = obj.GetType();
            return ret;
        }

        private void ValidateTrainingData(List<List<object>> X)
        {
            var expectedLength = X[0].Count;
            for (int i = 1; i < X.Count; i++)
            {
                if (X[i].Count != expectedLength)
                    throw new InvalidDataFormatException("Not all of the training data was the same length." +
                        "Length of X[" + i + "] was " + X[i].Count + " and should have been " + expectedLength);
            }
            for (int i = 0; i < expectedLength; i++)
            {
                foreach (List<object> currentData in X)
                {
                    if (TypeOfDataElement(X[0][i]) != TypeOfDataElement(currentData[i]))
                        throw new InvalidDataFormatException(
                            "Not all elements within the training data have the same type within index " + i
                        );
                }
            }
        }

        private void ValidateTargetData(List<object> Y, int expectedLength)
        {
            if (Y.Count != expectedLength)
                throw new InvalidDataFormatException(
                    "The lengths of the training and target data do not match"
                );
        }

        private void ValidateInputData(List<List<object>> X, List<object> Y)
        {
            ValidateTrainingData(X);
            ValidateTargetData(Y, X.Count);
        }



        private void SetupLabelDictionary(List<List<object>> x, int indexToFill, Dictionary<object, int> toFill)
        {
            if (KNNTypeDictionaries.USE_RAW.ContainsKey(x[0][indexToFill].GetType()))
                return;
            for (int i = 0; i < x.Count; i++)
            {
                if(!toFill.ContainsKey(x[i][indexToFill]))
                    toFill[x[i][indexToFill]] = toFill.Count;
            }
        }

        /**
         * <summary>Sets up a mapping for any data that is not a numeric value that converts it to a numeric value</summary>
         * This is done based on the index of the data:
         *   For instance, considering data like ['a', 'b', 0, 1, 2], then only 'a' and 'b' would need mappings generated
         *   However, as all data should be in the same format, 
         *   this triggers a mapping dictionary to be generated for all data in that position
         * When the number of label mapping dictionaries has been established,
         *   Then each data that needs to be mapped is given a number based on its order of appearance;
         *   So if 'a' was seen by the mapping algorithm first, then it is assigned the value 0, etc.
         */
        private void SetupLabelMapping(List<List<object>> X)
        {
            for (int i = 0; i < X[0].Count; i++)
            {
                var toFill = new Dictionary<object, int>();
                SetupLabelDictionary(X, i, toFill);
                if (toFill.Count == 0)
                    LabelMappingDictionaries.Add(null);
                else
                    LabelMappingDictionaries.Add(toFill);
            }
        }

        private void AddTrainingData(List<List<object>> X, List<object> Y)
        {
            for (int i = 0; i < X.Count; i++)
                DataPoints.Add(GenerateKNNDataPoint(X[i], Y[i]));
        }

        public void Train(List<List<object>> X, List<object> Y)
        {
            ValidateInputData(X, Y);
            SetupLabelMapping(X);
            AddTrainingData(X, Y);
        }

        private KNNDataPoint GenerateKNNDataPoint(List<object> x, object y)
        {
            List<double> dataPoints = new List<double>();
            var typeDictionary = KNNTypeDictionaries.USE_RAW;
            for (int i = 0; i < x.Count; i++)
            {
                if (typeDictionary.ContainsKey(x[i].GetType()))
                {
                    dataPoints.Add(Convert.ToDouble(x[i]));
                } else
                {
                    if (LabelMappingDictionaries[i] == null)
                        throw new InvalidDataFormatException(
                            "Input data format did not match trained data format for index " + i
                        );
                    try
                    {
                        dataPoints.Add(LabelMappingDictionaries[i][x[i]]);
                    }
                    catch (KeyNotFoundException)
                    {
                        LabelMappingDictionaries[i][x[i]] = LabelMappingDictionaries[i].Count;
                        dataPoints.Add(LabelMappingDictionaries[i][x[i]]);
                    }
                }
            }
            return new KNNDataPoint(dataPoints, y);
        }

        public void Load(string filePath)
        {
            KNNDataManager.LoadData(filePath, this, out List<Dictionary<object, int>> md, out List<KNNDataPoint> dp );
            LabelMappingDictionaries = md;
            DataPoints = dp;
        }

        public void Save(string filePath)
        {
            KNNDataManager.SaveData(filePath, this);
        }

        public List<Dictionary<object, int>> CopyLabelMappingDictionary()
        {
            List<Dictionary<object, int>> ret = new List<Dictionary<object, int>>();
            for (int i = 0; i < LabelMappingDictionaries.Count; i++) {
                var dict = LabelMappingDictionaries[i];
                Dictionary<object, int> toAdd = new Dictionary<object, int>();
                if (dict != null)
                {
                    foreach (object key in dict.Keys)
                    {
                        toAdd[key] = dict[key];
                    }
                }
                ret.Add(toAdd);
            }
            return ret;
        }

        public List<KNNDataPoint> CopyDataPoints()
        {
            List<KNNDataPoint> points = new List<KNNDataPoint>();
            foreach (KNNDataPoint currPoint in DataPoints)
            {
                points.Add(currPoint);
            }
            return points;
        }

        public object Predict(List<object> inputData, Func<List<double>, List<double>, double> distanceCalculationFunction )
        {
            return PredictTopN(inputData, distanceCalculationFunction, 1)[0];
        }

        private List<double> ConvertInputData(List<object> inputData)
        {
            if (inputData.Count != DataPoints[0].DataPoints.Count)
                throw new InvalidDataFormatException("Input data has a differing number of elements than all other data seen!");
            List<double> retPoint = new List<double>();
            for (int i = 0; i < inputData.Count; i++)
            {
                if (LabelMappingDictionaries[i] == null || LabelMappingDictionaries[i].Count == 0)
                {
                    try
                    {
                        retPoint.Add(Convert.ToDouble(inputData[i]));
                    }
                    catch (InvalidCastException)
                    {
                        throw new InvalidDataFormatException(
                            "Input data at position " + i + " is required to be numeric and was not"
                            );
                    }
                }
                else
                {
                    retPoint.Add(
                        LabelMappingDictionaries[i].GetValueOrDefault(
                                inputData[i],
                                LabelMappingDictionaries[i].Count
                            )
                    );
                }
            }
            return retPoint;
        }

        private List<KNNDistanceMapping> CalculateDistances(List<double> inputDataPoint, Func<List<double>, List<double>, double> distanceCalculationFunction)
        {
            List<KNNDistanceMapping> ret = new List<KNNDistanceMapping>();
            foreach (KNNDataPoint currPoint in DataPoints)
            {
                var currDataPoint = currPoint.DataPoints;
                double distance = distanceCalculationFunction(currDataPoint, inputDataPoint);
                ret.Add(new KNNDistanceMapping(distance, currPoint.ExpectedResult));
            }
            ret.Sort(KNNDistanceMapping.Compare);
            return ret;
        }

        public List<object> PredictTopN(List<object> inputData, Func<List<double>, List<double>, double> distanceCalculationFunction, int n)
        {
            List<double> inputDataPoint = ConvertInputData(inputData);
            var distances = CalculateDistances(inputDataPoint, distanceCalculationFunction);
            List<object> retObjects = new List<object>();
            if (n >= distances.Count)
                foreach (KNNDistanceMapping currMapping in distances)
                    retObjects.Add(currMapping.MappedObject);
            else
                for (int i = 0; i < n; i++)
                    retObjects.Add(distances[i].MappedObject);
            return retObjects;
        }
    }


    class KNNDistanceMapping
    {
        public double Distance { get; set; }
        public object MappedObject { get; set; }
        public KNNDistanceMapping(double distance, object mappedObject)
        {
            Distance = distance;
            MappedObject = mappedObject;
        }

        public static int Compare(KNNDistanceMapping a, KNNDistanceMapping b)
        {
            if (a.Distance < b.Distance)
                return -1;
            if (a.Distance > b.Distance)
                return 1;
            return 0;
        }
    }
}

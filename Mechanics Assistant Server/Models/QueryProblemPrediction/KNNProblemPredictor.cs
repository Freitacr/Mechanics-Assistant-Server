using System;
using System.Collections.Generic;
using System.IO;

namespace MechanicsAssistantServer.Models.QueryProblemPrediction
{
    public class KNNProblemPredictor : IQueryProblemPredictor
    {
        private KNN Model;

        public KNNProblemPredictor()
        {
            Model = new KNN();
        }

        public bool Load(string filePath)
        {
            try
            {
                Model = new KNN();
                Model.Load(filePath);
            } catch (IOException)
            {
                return false;
            }
            return true;
        }

        public object Predict(List<object> inputData, Func<List<double>, List<double>, double> distanceCalculationFunction)
        {
            return Model.Predict(inputData, distanceCalculationFunction);
        }

        public List<object> PredictTopN(List<object> inputData, Func<List<double>, List<double>, double> distanceCalculationFunction, int n)
        {
            return Model.PredictTopN(inputData, distanceCalculationFunction, n);
        }

        public bool Save(string filePath)
        {
            try
            {
                Model.Save(filePath);
            } catch(IOException)
            {
                return false;
            }
            return true;
        }

        public bool Train(List<List<object>> X, List<object> Y)
        {
            try
            {
                Model.Train(X, Y);
            } catch (InvalidDataFormatException)
            {
                return false;
            }
            return true;
        }
    }
}

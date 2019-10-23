using System;
using System.Collections.Generic;
using System.IO;

namespace OldManinTheShopServer.Models.QueryProblemPrediction
{
    /**KNN implementation of the IQueryProblemPredictor Interface*/
    public class KNNProblemPredictor : IQueryProblemPredictor
    {
        private KNN Model;

        public KNNProblemPredictor()
        {
            Model = new KNN();
        }

        public bool Load(Stream fileStream)
        {
            try
            {
                Model = new KNN();
                Model.Load(fileStream);
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

        public bool Save(Stream fileStream)
        {
            try
            {
                Model.Save(fileStream);
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

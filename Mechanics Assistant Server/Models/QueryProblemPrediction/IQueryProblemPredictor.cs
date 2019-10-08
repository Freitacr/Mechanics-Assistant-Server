using System;
using System.Collections.Generic;
using System.IO;

namespace MechanicsAssistantServer.Models.QueryProblemPrediction
{
    /**<summary>Interface to allow arbitrary use of a query problem predictor</summary>*/
    public interface IQueryProblemPredictor
    {

        bool Save(Stream filePath);

        bool Load(Stream filePath);

        bool Train(List<List<object>> X, List<object> Y);

        object Predict(List<object> inputData, Func<List<double>, List<double>, double> distanceCalculationFunction);

        List<object> PredictTopN(List<object> inputData, Func<List<double>, List<double>, double> distanceCalculationFunction, int n);

    }
}

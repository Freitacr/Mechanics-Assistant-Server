using System;
using System.Collections.Generic;
using System.Text;

namespace MechanicsAssistantServer.Models.KeywordPrediction
{
    public interface IKeywordPredictor
    {
        List<string> PredictKeywords(List<List<string>> sentencePosTokensIn);

        void Train(List<List<object>> X, List<object> Y);

        bool Load(string filePath);

        bool Save(string filePath);
    }
}

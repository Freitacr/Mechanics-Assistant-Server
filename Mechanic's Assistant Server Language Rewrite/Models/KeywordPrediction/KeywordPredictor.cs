using System;
using System.Collections.Generic;
using System.Text;

namespace MechanicsAssistantServer.Models.KeywordPrediction
{
    public interface KeywordPredictor
    {
        List<string> PredictKeywords(List<List<string>> sentencePosTokensIn);

        bool Load(string filePath);

        bool Save(string filePath);
    }
}

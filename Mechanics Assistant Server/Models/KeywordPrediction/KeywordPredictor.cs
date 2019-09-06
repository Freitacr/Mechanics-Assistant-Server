using System.Collections.Generic;
using System.IO;

namespace MechanicsAssistantServer.Models.KeywordPrediction
{
    public interface IKeywordPredictor
    {
        List<string> PredictKeywords(List<List<string>> sentencePosTokensIn);

        void Train(List<List<object>> X, List<object> Y);

        bool Load(Stream streamIn);

        bool Save(Stream filePath);
    }
}

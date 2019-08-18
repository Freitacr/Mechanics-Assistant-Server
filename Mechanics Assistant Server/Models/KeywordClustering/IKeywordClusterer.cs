using System.Collections.Generic;

namespace MechanicsAssistantServer.Models.KeywordClustering
{
    public interface IKeywordClusterer
    {
        void Train(List<KeywordExample> data);

        bool Load(string filePath);

        bool Save(string filePath);

        List<int> PredictGroupSimilarity(KeywordExample exampleIn);

        List<int> PredictTopNSimilarGroups(KeywordExample exampleIn, int n);
    }
}

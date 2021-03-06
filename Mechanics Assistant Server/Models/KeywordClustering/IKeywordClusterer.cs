﻿using System.Collections.Generic;
using System.IO;

namespace OldManInTheShopServer.Models.KeywordClustering
{
    /**Interface representing an arbitrary keyword clustering model*/
    public interface IKeywordClusterer
    {
        void Train(List<KeywordExample> data);

        bool Load(Stream streamIn);

        bool Save(Stream streamIn);

        List<int> PredictGroupSimilarity(KeywordExample exampleIn);

        List<int> PredictTopNSimilarGroups(KeywordExample exampleIn, int n);
    }
}

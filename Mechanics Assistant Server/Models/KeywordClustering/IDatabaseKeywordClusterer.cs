using OldManInTheShopServer.Data.MySql;
using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Models.KeywordClustering
{
    interface IDatabaseKeywordClusterer
    {

        void Train(List<KeywordExample> examplesIn);

        bool Load(MySqlDataManipulator manipulatorIn, int companyId, bool complaint = true);

        bool Save(MySqlDataManipulator manipulatorIn, int companyId, bool complaint = true);

        List<int> PredictTopNSimilarGroups(KeywordExample exampleIn, int numRequested);
    }
}

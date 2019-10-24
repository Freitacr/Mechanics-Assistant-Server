using OldManInTheShopServer.Data.MySql.TableDataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Models.QueryProblemPrediction
{
    interface IDatabaseQueryProblemPredictor
    {
        List<EntrySimilarity> GetQueryResults(JobDataEntry query, List<JobDataEntry> potentials, int numRequested, int offset = 0);
    }
}

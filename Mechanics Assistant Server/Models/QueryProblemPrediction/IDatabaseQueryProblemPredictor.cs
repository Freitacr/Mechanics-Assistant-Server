using OldManInTheShopServer.Data.MySql.TableDataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Models.QueryProblemPrediction
{
    /// <summary>
    /// Interface that all Database-based Query Problem Predictors must implement
    /// </summary>
    interface IDatabaseQueryProblemPredictor
    {
        List<EntrySimilarity> GetQueryResults(RepairJobEntry query, List<RepairJobEntry> potentials, int numRequested, int offset = 0);
    }
}

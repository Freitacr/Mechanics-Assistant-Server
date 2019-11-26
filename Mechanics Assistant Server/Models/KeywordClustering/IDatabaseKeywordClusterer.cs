using OldManInTheShopServer.Data.MySql;
using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Models.KeywordClustering
{
    /// <summary>
    /// Interface that all KeywordClusterers that work with the MySql database directly must implement from
    /// </summary>
    interface IDatabaseKeywordClusterer
    {
        /// <summary>
        /// Trains the IDatabaseKeywordClusterer based on the keyword examples passed in
        /// </summary>
        /// <param name="examplesIn">List of KeywordExample objects to use as training data</param>
        void Train(List<KeywordExample> examplesIn);

        /// <summary>
        /// Loads the IDatabaseKeywordClusterer from the database
        /// </summary>
        /// <param name="manipulatorIn">Object to use for accessing the database</param>
        /// <param name="companyId">Database id of the company to load the IDatabaseKeywordClusterer from</param>
        /// <param name="complaint">Whether this IDatabaseKeywordClusterer is dealing with complaint keyword groupings or problem keyword groupings</param>
        /// <returns>true if the model successfully loaded itself from the database, or false if it did not</returns>
        bool Load(MySqlDataManipulator manipulatorIn, int companyId);

        /// <summary>
        /// Attempts to save the IDatabaseKeywordClusterer to the database 
        /// </summary>
        /// <param name="manipulatorIn">Object to use for accessing the database</param>
        /// <param name="companyId">Database id of the company to save the IDatabaseKeywordClusterer to</param>
        /// <param name="complaint">Whether this IDatabaseKeywordClusterer is dealing with complaint keyword groupings or problem keyword groupings</param>
        /// <returns>true if the model successfully saved itself from the database, or false if it did not</returns>
        bool Save(MySqlDataManipulator manipulatorIn, int companyId);

        /// <summary>
        /// Attempts to predict the top <paramref name="numRequested"/> similar complaint groups to <paramref name="exampleIn"/>
        /// </summary>
        /// <param name="exampleIn">The keyword example to calculate the similarity of</param>
        /// <param name="numRequested">The number of similar groups to output</param>
        /// <returns>A list of sorted ints representing KeywordGroups by id based on similarity</returns>
        List<int> PredictTopNSimilarGroups(KeywordExample exampleIn, int numRequested);
    }
}

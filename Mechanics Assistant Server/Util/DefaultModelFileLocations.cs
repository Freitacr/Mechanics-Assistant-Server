namespace OldManInTheShopServer.Util
{
    /// <summary>
    /// Class that holds the responsiblity of storing the default file locations of
    /// the various models that this program uses.
    /// 
    /// Will soon be deprecated when the new prediction api is fully functional and integrated
    /// into the web service
    /// </summary>
    public static class DefaultModelFileLocations
    {
        public static readonly string POS_TAGGER_ENG_FILE = "InitialData\\averaged_perceptron_tagger.ans";
        public static readonly string NAIVE_BAYES_KEYWORD_PREDICTOR_FILE = "Models\\KeywordPredictorModel.ans";
        public static readonly string KEYWORD_SIMILARITY_CLUSTERER_FILE = "Models\\KeywordSimilarityClusterer.ans";
        public static readonly string KNN_QUERY_PROBLEM_PREDICTOR_FILE = "Models\\KNNProblemPredictor.ans";
    }
}

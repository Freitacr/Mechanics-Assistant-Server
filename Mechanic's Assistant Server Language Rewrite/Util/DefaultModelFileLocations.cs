using System;
using System.Collections.Generic;
using System.Text;

namespace MechanicsAssistantServer.Util
{
    public static class DefaultModelFileLocations
    {
        public static readonly string POS_TAGGER_ENG_FILE = "Models\\averaged_perceptron_tagger.json";
        public static readonly string NAIVE_BAYES_KEYWORD_PREDICTOR_FILE = "Models\\KeywordPredictorModel.nbmdl";
        public static readonly string KEYWORD_SIMILARITY_CLUSTERER_FILE = "Models\\KeywordSimilarityClusterer.kcmdl";
        public static readonly string KNN_QUERY_PROBLEM_PREDICTOR_FILE = "Models\\KNNProblemPredictor.knnmdl";
    }
}

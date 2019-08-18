using System.Collections.Generic;
using MechanicsAssistantServer.Data;

namespace MechanicsAssistantServer.Models.KeywordPrediction
{
    public class KeywordPredictorTrainingUtils
    {
        public static List<List<object>> GenerateKeywordTrainingData(List<KeywordTrainingExample> examplesIn)
        {
            List<List<object>> ret = new List<List<object>>();
            foreach (KeywordTrainingExample ex in examplesIn)
            {
                List<object> currentExamplePOS = new List<object>();
                foreach (var pair in ex.KeywordPairs)
                    currentExamplePOS.Add(pair.Pos);
                ret.Add(currentExamplePOS);
            }
            return ret;
        }

        public static List<object> GenerateKeywordTargetData(List<KeywordTrainingExample> examplesIn)
        {
            List<object> ret = new List<object>();
            foreach (KeywordTrainingExample ex in examplesIn)
                ret.Add(ex.IsCorrect);
            return ret;
        }
    }
}

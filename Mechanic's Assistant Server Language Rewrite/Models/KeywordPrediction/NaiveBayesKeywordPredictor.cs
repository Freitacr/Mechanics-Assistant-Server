using System;
using System.Collections.Generic;
using System.IO;

namespace MechanicsAssistantServer.Models.KeywordPrediction
{
    public class NaiveBayesKeywordPredictor : KeywordPredictor
    {
        private NaiveBayes Model;

        private List<List<string>> SplitInputIntoExamples(List<List<string>> sentencePosTokensIn)
        {
            sentencePosTokensIn.Insert(0, new List<string> { "START", "START" });
            sentencePosTokensIn.Add(new List<string> { "END", "END" });
            List<string> partOfSpeechTags = new List<string>();
            foreach (List<string> posPair in sentencePosTokensIn)
                partOfSpeechTags.Add(posPair[1]);
            List<List<string>> retExamples = new List<List<string>>();
            for (int i = 0; i < sentencePosTokensIn.Count-3; i++)
                retExamples.Add(partOfSpeechTags.GetRange(i, 3));
            return retExamples;
        }

        public bool Load(string filePath)
        {
            Model = new NaiveBayes();
            try
            {
                Model.Load(filePath);
            } catch (FileNotFoundException)
            {
                return false;
            }
            return true;
        }

        public List<string> PredictKeywords(List<List<string>> sentencePosTokensIn)
        {
            List<string> retKeywords = new List<string>();
            var examples = SplitInputIntoExamples(sentencePosTokensIn);
            int currIndex = 1; //the element directly after the START token
            foreach(List<string> example in examples)
            {
                List<object> objExample = new List<object>();
                foreach (string x in example)
                    objExample.Add(x);
                object res = Model.Predict(objExample);
                if ((bool)res)
                    retKeywords.Add(sentencePosTokensIn[currIndex][0]);
                currIndex++;
            }
            return retKeywords;
        }

        public bool Save(string filePath)
        {
            try
            {
                Model.Save(filePath);
            } catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}

﻿using System.Collections.Generic;

/** 
 * An averaged perceptron, as implemented by Matthew Honnibal.
 * See more implementation details here:
 * https://explosion.ai/blog/part-of-speech-pos-tagger-in-python
 * Author: Matthew Honnibal <honnibal+gh@gmail.com>
 * Copyright 2013 Matthew Honnibal
 * This module is provided under the terms of the MIT License.
*/

namespace OldManInTheShopServer.Models.POSTagger
{
    class AveragedPerceptron
    {
        private AveragedPerceptronTagger ParentTagger;
        public AveragedPerceptron(AveragedPerceptronTagger parent)
        {
            ParentTagger = parent;
        }

        public string Predict(Dictionary<string, double> features)
        {
            Dictionary<string, double> scores = new Dictionary<string, double>();
            foreach(KeyValuePair<string, double> pair in features)
            {
                if (!ParentTagger.WeightDictionary.ContainsKey(pair.Key))
                    continue;
                if (pair.Value == 0.0)
                    continue;
                Dictionary<string, double> weights = ParentTagger.WeightDictionary[pair.Key];
                foreach (KeyValuePair<string, double> weightPair in weights)
                {
                    if (!scores.ContainsKey(weightPair.Key))
                        scores[weightPair.Key] = 0;
                    scores[weightPair.Key] += weightPair.Value * pair.Value;
                }
            }

            //Find maximum value. If two labels have the same value, then compare the strings to see which is alphabetically first, for stability.
            string retLabel = "";
            double maxValue = 0.0;
            foreach(KeyValuePair<string, double> pair in scores)
            {
                if (pair.Value > maxValue)
                {
                    retLabel = pair.Key;
                    maxValue = pair.Value;
                } else if(pair.Value == maxValue)
                {
                    if (retLabel.CompareTo(pair.Key) < 0)
                        retLabel = pair.Key;
                }
            }
            return retLabel;
        }
    }
}

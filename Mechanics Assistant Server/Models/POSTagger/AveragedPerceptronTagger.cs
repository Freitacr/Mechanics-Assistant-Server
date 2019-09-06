using System.Collections.Generic;
using System.IO;
using MechanicsAssistantServer.Util;

namespace MechanicsAssistantServer.Models.POSTagger
{
    public class AveragedPerceptronTagger
    {
        private readonly static string[] START_TOKENS = new string[] { "-START-", "-START2-" }; 
        private readonly static string[] END_TOKENS = new string[] { "-END-", "-END2-" }; 
        public Dictionary<string, Dictionary<string, double>> WeightDictionary { get; private set; }
        private Dictionary<string, string> KnownTags;
        public List<string> Classes { get; private set; }
        private AveragedPerceptron Model;
        public static AveragedPerceptronTagger Load(Stream fileStreamIn)
        {
            StreamReader textReader = new StreamReader(fileStreamIn);
            List<object> dataContents = CustomJsonParser.ParseList(textReader);
            AveragedPerceptronTagger tagger = new AveragedPerceptronTagger();
            tagger.ConvertWeightDictionary((Dictionary<object, object>)dataContents[0]);
            tagger.KnownTags = tagger.ConvertKnownTagsDictionary((Dictionary<object, object>)dataContents[1]);
            tagger.ConvertClassesList((List<object>)dataContents[2]);
            tagger.Model = new AveragedPerceptron(tagger);
            return tagger;
        }

        private void ConvertWeightDictionary(Dictionary<object, object> unconvertedDict)
        {
            WeightDictionary = new Dictionary<string, Dictionary<string, double>>();
            foreach(KeyValuePair<object, object> pair in unconvertedDict)
            {
                string key = (string)pair.Key;
                Dictionary<string, double> value = ConvertSubWeightDictionary((Dictionary<object, object>)pair.Value);
                WeightDictionary[key] = value;
            }
        }

        private Dictionary<string, double> ConvertSubWeightDictionary(Dictionary<object, object> unconvertedDict)
        {
            Dictionary<string, double> ret = new Dictionary<string, double>();
            foreach(KeyValuePair<object, object> pair in unconvertedDict)
            {
                string key = (string)pair.Key;
                double value = double.Parse((string)pair.Value);
                ret[key] = value;
            }
            return ret;
        }

        private Dictionary<string, string> ConvertKnownTagsDictionary(Dictionary<object, object> unconvertedDict)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach(KeyValuePair<object, object> pair in unconvertedDict)
            {
                string key = (string)pair.Key;
                string value = (string)pair.Value;
                ret[key] = value;
            }
            return ret;
        }

        private void ConvertClassesList(List<object> unconvertedList)
        {
            Classes = new List<string>();
            foreach (object str in unconvertedList)
                Classes.Add((string)str);
        }
        
        private string NormalizeString(string stringIn)
        {
            bool isStrictlyNumeric()
            {
                foreach (char c in stringIn)
                    if (!char.IsDigit(c))
                        return false;
                return true;
            }

            if (stringIn.Contains('-') && stringIn[0] != '-')
                return "!HYPHEN";
            else if (isStrictlyNumeric())
            {
                if (stringIn.Length == 4)
                    return "!YEAR";
                else
                    return "!DIGITS";
            }
            else
                return stringIn.ToLower();
        }

        public List<List<string>> Tag(List<string> tokens)
        {
            List<List<string>> ret = new List<List<string>>();
            List<string> tokensCopy = new List<string>();

            for (int i = 0; i < tokens.Count; i++)
                tokensCopy.Add(NormalizeString(tokens[i]));

            string previousTag = START_TOKENS[0];
            string secondPreviousTag = START_TOKENS[1];
            tokensCopy.Insert(0, secondPreviousTag);
            tokensCopy.Insert(0, previousTag);
            tokensCopy.Add(END_TOKENS[0]);
            tokensCopy.Add(END_TOKENS[1]);
            for(int i = 2; i < tokensCopy.Count - 2; i++)
            {
                string tag = KnownTags.GetValueOrDefault(tokensCopy[i], null);
                if (tag == null)
                {
                    var features = GetFeatures(i, tokensCopy, previousTag, secondPreviousTag);
                    tag = Model.Predict(features);
                }
                ret.Add(
                    new List<string> {tokens[i-2], tag}
                );
                secondPreviousTag = previousTag;
                previousTag = tag;
            }
            return ret;
        }

        private Dictionary<string, double> GetFeatures(int i, List<string> tokens, string previousTag, string secondPreviousTag)
        {
            Dictionary<string, double> ret = new Dictionary<string, double>();

            string GetSuffix(string strIn)
            {
                if (strIn.Length <= 3)
                    return strIn;
                string retStr = "";
                for (int j = strIn.Length - 3; j < strIn.Length; j++)
                    retStr += strIn[j];
                return retStr;
            }

            void add(string featureName, params string[] args)
            {
                string featureKey = featureName;
                if (args != null)
                {
                    if (args.Length > 0)
                    {
                        featureKey += ' ';
                        featureKey += string.Join(' ', args);
                    }
                }
                ret[featureKey] = 1;
            }

            add("bias");
            add("i suffix", GetSuffix(tokens[i]));
            add("i pref1", tokens[i][0] + "");
            add("i-1 tag", previousTag);
            add("i-2 tag", secondPreviousTag);
            add("i tag+i-2 tag", previousTag, secondPreviousTag);
            add("i word", tokens[i]);
            add("i-1 tag+i word", previousTag, tokens[i]);
            add("i-1 word", tokens[i - 1]);
            add("i-1 suffix", GetSuffix(tokens[i - 1]));
            add("i-2 word", tokens[i - 2]);
            add("i+1 word", tokens[i + 1]);
            add("i+1 suffix", GetSuffix(tokens[i + 1]));
            add("i+2 word", tokens[i + 2]);

            return ret;
            
        }
    }
}

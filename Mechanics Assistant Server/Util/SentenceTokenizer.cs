using System.Collections.Generic;

namespace MechanicsAssistantServer.Util
{
    public static class SentenceTokenizer
    {

        public static List<string> TokenizeSentence(string sentenceIn)
        {
            List<string> ret = new List<string>();
            string[] splitSentence = sentenceIn.Split(' ');
            foreach(string word in splitSentence)
            {
                if (word == "")
                    continue;
                string currWord = word;
                string punc = null;
                if (char.IsPunctuation(currWord[currWord.Length-1]))
                {
                    punc = currWord[currWord.Length - 1] + "";
                    currWord = currWord.Remove(currWord.Length - 1);
                }
                
                string contraction = null;
                if (currWord.EndsWith("n't") || currWord.EndsWith("'ve") || currWord.EndsWith("'re"))
                {
                    contraction = currWord.Substring(currWord.Length - 3, 3);
                } else if (currWord.EndsWith("'d"))
                {
                    contraction = "'d";
                }
                if (contraction != null)
                    currWord = currWord.Remove(currWord.Length - (contraction.Length));

                ret.Add(currWord);
                if (contraction != null)
                    ret.Add(contraction);
                if (punc != null)
                    ret.Add(punc);
            }
            return ret;
        }

        
    }
}

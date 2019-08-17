using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text;

namespace MechanicsAssistantServer.Data
{
    [DataContract]
    public class KeywordTrainingExample
    {
        [DataContract]
        public class KeywordPair
        {
            [DataMember(Name = "word")]
            public string Word { get; set; }

            [DataMember(Name = "pos")]
            public string Pos { get; set; }
            public KeywordPair(string word, string pos)
            {
                Word = word;
                Pos = pos;
            }

            public override string ToString()
            {
                StringBuilder retBuilder = new StringBuilder("word: " + Word);
                retBuilder.Append("\npos: " + Pos);
                return retBuilder.ToString();
            }

            // override object.Equals
            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                KeywordPair other = obj as KeywordPair;
                if (other.Word != this.Word)
                    return false;
                return other.Pos == this.Pos;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        [DataMember(Name = "data")]
        public List<KeywordPair> KeywordPairs { get; set; }

        [DataMember(Name = "correct")]
        public bool IsCorrect { get; set; }

        public KeywordTrainingExample(List<KeywordPair> keywordPairs, bool isCorrect)
        {
            KeywordPairs = keywordPairs;
            IsCorrect = isCorrect;
        }

        public override string ToString()
        {
            StringBuilder retBuilder = new StringBuilder(IsCorrect.ToString());
            foreach (KeywordPair currPair in KeywordPairs)
            {
                retBuilder.Append(",\n");
                retBuilder.Append(currPair.ToString());
            }
            return retBuilder.ToString();
        }


        // override object.Equals
        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // TODO: write your implementation of Equals() here
            KeywordTrainingExample other = obj as KeywordTrainingExample;
            if (other.KeywordPairs.Count != KeywordPairs.Count)
                return false;
            for (int i = 0; i < KeywordPairs.Count; i++)
                if (!KeywordPairs[i].Equals(other.KeywordPairs[i]))
                    return false;
            return other.IsCorrect == this.IsCorrect;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

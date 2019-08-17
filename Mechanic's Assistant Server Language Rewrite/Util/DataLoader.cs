using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace MechanicsAssistantServer.Util
{
   
    [DataContract]
    public class MechanicQuery
    {
        [DataMember(Name = "make")]
        public string Make { get; set; }

        [DataMember(Name = "model")]
        public string Model { get; set; }

        [DataMember(Name = "problem", IsRequired = false)]
        public string Problem { get; set; }

        [DataMember(Name = "complaint")]
        public string Complaint { get; set; }

        [DataMember(Name = "vin", IsRequired = false)]
        public string Vin { get; set; }

        public MechanicQuery(string make, string model, string problem, string vin, string complaint)
        {
            Make = make;
            Model = model;
            Problem = problem;
            Vin = vin;
            Complaint = complaint;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            MechanicQuery other = obj as MechanicQuery;
            if (Make != other.Make)
                return false;
            if (Model != other.Model)
                return false;
            if (Vin != other.Vin)
                return false;
            if (Complaint != other.Complaint)
                return false;
            return Problem == other.Problem;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            int ret = (Make ?? "").GetHashCode();
            ret += (Model ?? "").GetHashCode();
            ret += (Vin ?? "").GetHashCode();
            ret += (Complaint ?? "").GetHashCode();
            ret += (Problem ?? "").GetHashCode();
            return ret;
            // TODO: write your implementation of GetHashCode() here
            
            
        }
    }

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
            foreach(KeywordPair currPair in KeywordPairs) {
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

    public class DataLoader
    {

        public static List<KeywordTrainingExample> LoadKeywordTrainingExamples(string keywordDataFilePath)
        {
            DataContractJsonSerializer keywordDataSerializer = new DataContractJsonSerializer(
                typeof(List<KeywordTrainingExample>)
            );
            StreamReader keywordFileReader = new StreamReader(keywordDataFilePath);
            List<KeywordTrainingExample> keywordList = (List<KeywordTrainingExample>)keywordDataSerializer
                .ReadObject(keywordFileReader.BaseStream);
            keywordFileReader.Close();
            return keywordList;
        }

        public static List<MechanicQuery> LoadMechanicQueries(string queryFilePath)
        {
            DataContractJsonSerializer querySerializer = new DataContractJsonSerializer(
                typeof(List<MechanicQuery>)
                );
            StreamReader queryFileReader = new StreamReader(queryFilePath);
            List<MechanicQuery> retList = (List<MechanicQuery>)querySerializer
                .ReadObject(queryFileReader.BaseStream);
            queryFileReader.Close();
            return retList;
        }
    }
}

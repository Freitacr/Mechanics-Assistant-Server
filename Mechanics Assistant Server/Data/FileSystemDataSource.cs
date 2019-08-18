using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace MechanicsAssistantServer.Data
{
    class FileSystemDataSource : DataSource
    {
        private static readonly string DEFAULT_KEYWORD_DATA_FILE_PATH = "Data\\formattedKeywordTrainingData.txt";
        private static readonly string DEFAULT_MECHANIC_QUERY_FILE_PATH = "Data\\mechanicQueries.txt";

        public override List<KeywordTrainingExample> LoadKeywordTrainingExamples()
        {
            DataContractJsonSerializer keywordDataSerializer = new DataContractJsonSerializer(
                typeof(List<KeywordTrainingExample>)
            );
            StreamReader keywordFileReader = new StreamReader(DEFAULT_KEYWORD_DATA_FILE_PATH);
            List<KeywordTrainingExample> keywordList = (List<KeywordTrainingExample>)keywordDataSerializer
                .ReadObject(keywordFileReader.BaseStream);
            keywordFileReader.Close();
            return keywordList;
        }

        public override List<MechanicQuery> LoadMechanicQueries()
        {
            DataContractJsonSerializer querySerializer = new DataContractJsonSerializer(
                typeof(List<MechanicQuery>)
                );
            StreamReader queryFileReader = new StreamReader(DEFAULT_MECHANIC_QUERY_FILE_PATH);
            List<MechanicQuery> retList = (List<MechanicQuery>)querySerializer
                .ReadObject(queryFileReader.BaseStream);
            queryFileReader.Close();
            return retList;
        }

        public override bool AddData(MechanicQuery toAdd)
        {
            DataContractJsonSerializer querySerializer = new DataContractJsonSerializer(
                typeof(List<MechanicQuery>)
                );

            StreamReader queryFileReader;
            try
            {
                queryFileReader = new StreamReader(DEFAULT_MECHANIC_QUERY_FILE_PATH);
            } catch (IOException)
            {
                return false;
            }
            List<MechanicQuery> retList = (List<MechanicQuery>)querySerializer
                .ReadObject(queryFileReader.BaseStream);
            queryFileReader.Close();
            retList.Add(toAdd);
            StreamWriter queryFileWriter;
            try
            {
                queryFileWriter = new StreamWriter(DEFAULT_MECHANIC_QUERY_FILE_PATH);
            } catch (IOException)
            {
                return false;
            }
            try
            {
                querySerializer.WriteObject(queryFileWriter.BaseStream, retList);
            } catch (SerializationException)
            {
                retList.RemoveAt(retList.Count - 1);
                querySerializer.WriteObject(queryFileWriter.BaseStream, retList);
                return false;
            }
            queryFileWriter.Close();
            return true;
        }
    }
}

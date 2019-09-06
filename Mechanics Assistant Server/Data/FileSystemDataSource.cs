﻿using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using EncodingUtilities;
using ANSEncodingLib;

namespace MechanicsAssistantServer.Data
{
    class FileSystemDataSource : DataSource
    {
        private static readonly string DEFAULT_KEYWORD_DATA_FILE_PATH = "Data\\formattedKeywordTrainingData.ans";
        private static readonly string DEFAULT_MECHANIC_QUERY_FILE_PATH = "Data\\mechanicQueries.ans";

        public override List<KeywordTrainingExample> LoadKeywordTrainingExamples()
        {
            var readerIn = new FileStream(DEFAULT_KEYWORD_DATA_FILE_PATH, FileMode.Open, FileAccess.Read);
            MemoryStream memoryStreamOut = new MemoryStream();
            AnsBlockDecoder decoder = new AnsBlockDecoder(memoryStreamOut);
            decoder.DecodeStream(readerIn);
            readerIn.Close();
            MemoryStream memoryStreamIn = new MemoryStream(memoryStreamOut.ToArray());
            DataContractJsonSerializer keywordDataSerializer = new DataContractJsonSerializer(
                typeof(List<KeywordTrainingExample>)
            );
            StreamReader keywordFileReader = new StreamReader(memoryStreamIn);
            List<KeywordTrainingExample> keywordList = (List<KeywordTrainingExample>)keywordDataSerializer
                .ReadObject(keywordFileReader.BaseStream);
            keywordFileReader.Close();
            memoryStreamOut.Close();
            return keywordList;
        }

        public override List<MechanicQuery> LoadMechanicQueries()
        {
            var readerIn = new FileStream(DEFAULT_MECHANIC_QUERY_FILE_PATH, FileMode.Open, FileAccess.Read);
            MemoryStream memoryStreamOut = new MemoryStream();
            AnsBlockDecoder decoder = new AnsBlockDecoder(memoryStreamOut);
            decoder.DecodeStream(readerIn);
            readerIn.Close();
            MemoryStream memoryStreamIn = new MemoryStream(memoryStreamOut.ToArray());
            DataContractJsonSerializer querySerializer = new DataContractJsonSerializer(
                typeof(List<MechanicQuery>)
                );
            StreamReader queryFileReader = new StreamReader(memoryStreamIn);
            List<MechanicQuery> retList = (List<MechanicQuery>)querySerializer
                .ReadObject(queryFileReader.BaseStream);
            queryFileReader.Close();
            memoryStreamOut.Close();
            return retList;
        }

        public override bool AddData(MechanicQuery toAdd)
        {
            DataContractJsonSerializer querySerializer = new DataContractJsonSerializer(
                typeof(List<MechanicQuery>)
                );
            List<MechanicQuery> retList = LoadMechanicQueries();
            retList.Add(toAdd);

            MemoryStream streamOut = new MemoryStream();
            try
            {
                querySerializer.WriteObject(streamOut, retList);
            } catch (SerializationException)
            {
                return false;
            }
            BitWriter writerOut = new BitWriter(
                new FileStream(DEFAULT_MECHANIC_QUERY_FILE_PATH, FileMode.Create, FileAccess.Write)
            );
            streamOut = new MemoryStream(streamOut.ToArray());
            AnsBlockEncoder encoder = new AnsBlockEncoder(1048576, writerOut);
            encoder.EncodeStream(streamOut, 8);
            writerOut.Flush();
            writerOut.Close();
            return true;
        }
    }
}
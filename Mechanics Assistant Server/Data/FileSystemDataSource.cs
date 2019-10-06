using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using EncodingUtilities;
using ANSEncodingLib;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Mechanics Assistant Server Tests")]

namespace MechanicsAssistantServer.Data
{
    class FileSystemDataSource : DataSource
    {
        private static readonly string DEFAULT_KEYWORD_DATA_FILE_PATH = "InitialData\\formattedKeywordTrainingData.ans";
        private static readonly string DEFAULT_MECHANIC_QUERY_FILE_PATH = "InitialData\\mechanicQueries.ans";

        public string KeywordDataFilePath { get; set; } = DEFAULT_KEYWORD_DATA_FILE_PATH;
        public string MechanicQueryFilePath { get; set; } = DEFAULT_MECHANIC_QUERY_FILE_PATH;

        public override List<KeywordTrainingExample> LoadKeywordTrainingExamples()
        {
            var readerIn = new FileStream(KeywordDataFilePath, FileMode.Open, FileAccess.Read);
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
            var readerIn = new FileStream(MechanicQueryFilePath, FileMode.Open, FileAccess.Read);
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
                new FileStream(MechanicQueryFilePath, FileMode.Create, FileAccess.Write)
            );
            streamOut = new MemoryStream(streamOut.ToArray());
            AnsBlockEncoder encoder = new AnsBlockEncoder(1048576, writerOut);
            encoder.EncodeStream(streamOut, 8);
            writerOut.Flush();
            writerOut.Close();
            return true;
        }

        public bool AddKeywordExample(KeywordTrainingExample ex)
        {
            DataContractJsonSerializer querySerializer = new DataContractJsonSerializer(
                typeof(List<KeywordTrainingExample>)
                );
            List<KeywordTrainingExample> retList = LoadKeywordTrainingExamples();
            retList.Add(ex);

            MemoryStream streamOut = new MemoryStream();
            try
            {
                querySerializer.WriteObject(streamOut, retList);
            }
            catch (SerializationException)
            {
                return false;
            }
            BitWriter writerOut = new BitWriter(
                new FileStream(KeywordDataFilePath, FileMode.Create, FileAccess.Write)
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

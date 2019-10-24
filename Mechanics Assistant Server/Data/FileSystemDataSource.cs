using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using EncodingUtilities;
using ANSEncodingLib;
using System.Runtime.CompilerServices;

/**
 * This exists to provide the testing project access to the non public classes within this file in DEBUG MODE ONLY. In release mode the testing project will not have
 * access to these classes.
 */

#if DEBUG
    [assembly: InternalsVisibleTo("Mechanics Assistant Server Tests")]
#endif


namespace OldManInTheShopServer.Data
{
    /**
     * <summary>Data Source that gets its backing data from Asymmetric Numeric System encoded files on the hard drive.</summary>
     */
    class FileSystemDataSource : DataSource
    {
        private static readonly string DEFAULT_KEYWORD_DATA_FILE_PATH = "InitialData\\formattedKeywordTrainingData.ans";
        private static readonly string DEFAULT_MECHANIC_QUERY_FILE_PATH = "InitialData\\mechanicQueries.ans";

        /** <summary>Current Keyword data file path. Will default to DEFAULT_KEYWORD_DATA_FILE_PATH if not set externally</summary>*/
        public string KeywordDataFilePath { get; set; } = DEFAULT_KEYWORD_DATA_FILE_PATH;
        /** <summary>Current Mechanic Query Data file path. Will default to DEFAULT_KEYWORD_DATA_FILE_PATH if not set externally</summary>*/
        public string MechanicQueryFilePath { get; set; } = DEFAULT_MECHANIC_QUERY_FILE_PATH;

        /** <summary>Loads Stored KeywordTrainingExamples from the file specified by KeywordDataFilePath</summary>*/
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
        /** <summary>Loads Mechanic Queries from the file specified by MechanicQueryFilePath</summary>*/
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

        /** 
         * <summary>Attempts to add the MechanicQuery specified by toAdd to the file MechanicQueryFilePath</summary>
         * <param name="toAdd">Mechanic Query to add</param>
         */
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

        /** 
         * <summary>Attempts to add the KeywordTrainingExample specified by toAdd to the file KeywordDataFilePath</summary>
         * <param name="ex">KeywordTrainingExample to add</param>
         */
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

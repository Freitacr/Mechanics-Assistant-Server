using ANSEncodingLib;
using OldManInTheShopServer.Data;
using OldManInTheShopServer.Models.KeywordPrediction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OldManInTheShopServer.Util
{
    /// <summary>
    /// Helper class designed to assist in loading or training and saving the global IKeywordPredictor models.
    /// </summary>
    class GlobalModelHelper
    {

        public static bool LoadOrTrainGlobalModels(IEnumerable<KeywordPredictorFileMapping> models)
        {
            
            foreach(KeywordPredictorFileMapping mapping in models)
            {
                if (!LoadModel(mapping))
                    if (!TrainModel(mapping))
                        return false;
            }
            return true;
        }

        private static bool LoadModel(KeywordPredictorFileMapping mapping)
        {
            IKeywordPredictor predictor = mapping.KeywordPredictorType.GetMethod("GetGlobalModel").Invoke(null, null) as IKeywordPredictor;
            Stream streamIn = null;
            try
            {
                streamIn = new AnsDecoderStream(new FileStream(mapping.DefaultFileLocation, FileMode.Open, FileAccess.Read));
            } catch (FileNotFoundException)
            {
                return false;
            }
            using (streamIn)
            {
                return predictor.Load(streamIn);
            }
        }

        private static bool TrainModel(KeywordPredictorFileMapping mapping)
        {
            IKeywordPredictor predictor = mapping.KeywordPredictorType.GetMethod("GetGlobalModel").Invoke(null, null) as IKeywordPredictor;
            var keywordData = new FileSystemDataSource().LoadKeywordTrainingExamples();
            var X = KeywordPredictorTrainingUtils.GenerateKeywordTrainingData(keywordData);
            var Y = KeywordPredictorTrainingUtils.GenerateKeywordTargetData(keywordData);
            predictor.Train(X, Y);
            AnsEncoderStream stream;
            try
            {
                stream = new AnsEncoderStream(
                new FileStream(mapping.DefaultFileLocation, FileMode.Create, FileAccess.Write),
                1048576,
                4096);
            } catch (IOException)
            {
                return false;
            }
            using (stream)
            {
                bool res = predictor.Save(stream);
                stream.Flush();
                return res;
            }
        }
    }
}

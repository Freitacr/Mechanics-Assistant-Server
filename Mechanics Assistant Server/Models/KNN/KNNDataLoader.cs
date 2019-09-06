using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;

namespace MechanicsAssistantServer.Models
{
    [DataContract]
    class KNNData
    {
        [DataMember]
        public List<Dictionary<object, int>> LabelMappingDictionary { get; set; }
        [DataMember]
        public List<KNNDataPoint> DataPoints { get; set; }

        public KNNData(List<Dictionary<object, int>> labelMappingDictionary, List<KNNDataPoint> dataPoints)
        {
            LabelMappingDictionary = labelMappingDictionary;
            DataPoints = dataPoints;
        }


    }

    static class KNNDataManager
    {
        public static void LoadData(Stream fileStream, KNN model, out List<Dictionary<object, int>> labelMappingDict, out List<KNNDataPoint> dataPoints)
        {
            StreamReader fileReader = new StreamReader(fileStream);
            DataContractJsonSerializer dataSerializer = new DataContractJsonSerializer(typeof(KNNData));
            KNNData data = (KNNData) dataSerializer.ReadObject(fileReader.BaseStream);
            labelMappingDict = data.LabelMappingDictionary;
            dataPoints = data.DataPoints;
            fileReader.Close();
        }

        public static void SaveData(Stream fileStream, KNN model)
        {
            KNNData data = new KNNData(
                model.CopyLabelMappingDictionary(),
                model.CopyDataPoints()
                );
            //As a note, this method assumes that all objects inside of each of the dictionaries are serializable by a DataContractJsonSerializer
            DataContractJsonSerializer dataSerializer = new DataContractJsonSerializer(data.GetType());
            StreamWriter fileWriter = new StreamWriter(fileStream);
            dataSerializer.WriteObject(fileWriter.BaseStream, data);
            fileWriter.Close();
        }
    }
}

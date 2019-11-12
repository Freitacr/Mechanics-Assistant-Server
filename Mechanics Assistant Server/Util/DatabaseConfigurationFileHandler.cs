using System;
using System.Collections.Generic;
using System.IO;
using ANSEncodingLib;
using EncodingUtilities;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Text;

namespace OldManInTheShopServer.Util
{
    [DataContract]
    class DatabaseConfigurationFileContents
    {
        [DataMember]
        public string User;

        [DataMember]
        public string Database;

        [DataMember]
        public string Host;

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Pass;
    }

    static class DatabaseConfigurationFileHandler
    {
        public static string ConfigurationFileLocation = "./config.json";

        public static DatabaseConfigurationFileContents LoadConfigurationFile()
        {
            ANSEncodingLib.AnsBlockDecoder fileDecoder;
            fileDecoder = new ANSEncodingLib.AnsBlockDecoder(new MemoryStream());
            FileStream fileReader;
            try
            {
                fileReader = new FileStream(ConfigurationFileLocation, FileMode.Open, FileAccess.Read);
            } catch (FileNotFoundException)
            {
                return null;
            }
            using (fileReader)
            {
                string fileContents;
                fileDecoder.DecodeStream(fileReader);
                fileDecoder.Output.Close();
                byte[] fileBytes = ((MemoryStream)fileDecoder.Output).ToArray();
                fileContents = Encoding.UTF8.GetString(fileBytes);
                return JsonDataObjectUtil<DatabaseConfigurationFileContents>.ParseObject(fileContents);
            }
        }

        public static DatabaseConfigurationFileContents GenerateDefaultConfiguration()
        {
            return new DatabaseConfigurationFileContents()
            {
                User = "omis",
                Database = "omis_data",
                Host = "localhost"
            };
        }

        public static bool WriteConfigurationFile(DatabaseConfigurationFileContents fileContents)
        {
            string contents = JsonDataObjectUtil<DatabaseConfigurationFileContents>.ConvertObject(fileContents);
            if (contents == null)
                return false;
            AnsBlockEncoder fileWriter;
            BitWriter writerOut;
            try
            {
                writerOut = new BitWriter(new FileStream(ConfigurationFileLocation, FileMode.Create, FileAccess.Write));
                fileWriter = new AnsBlockEncoder(1024, writerOut);
            } catch (IOException)
            {
                return false;
            }
            byte[] toWrite = Encoding.UTF8.GetBytes(contents);
            MemoryStream streamOut = new MemoryStream(toWrite);
            fileWriter.EncodeStream(streamOut, 8);
            writerOut.Close();
            return true;
        }
    }
}

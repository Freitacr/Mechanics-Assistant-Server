using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

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
            StreamReader fileReader;
            try
            {
                fileReader = new StreamReader(ConfigurationFileLocation);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            string fileContents;
            using (fileReader)
                fileContents = fileReader.ReadToEnd();
            return JsonDataObjectUtil<DatabaseConfigurationFileContents>.ParseObject(fileContents);
        }

        public static bool WriteConfigurationFileDefaults()
        {
            var fileContents = new DatabaseConfigurationFileContents()
            {
                User = "omis",
                Database = "omis_data",
                Host = "localhost"
            };
            string contents = JsonDataObjectUtil<DatabaseConfigurationFileContents>.ConvertObject(fileContents);
            if (contents == null)
                return false;
            StreamWriter fileWriter;
            try
            {
                fileWriter = new StreamWriter(ConfigurationFileLocation);
            } catch (IOException)
            {
                return false;
            }
            using (fileWriter)
                fileWriter.Write(contents);
            return true;
        }
    }
}

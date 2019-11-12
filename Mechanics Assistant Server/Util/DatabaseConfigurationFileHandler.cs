using System;
using System.Collections.Generic;
using System.IO;
using ANSEncodingLib;
using EncodingUtilities;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Text;
using System.Security;

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
        public SecureString Pass;

        public string Serialize()
        {
            StringBuilder retBuilder = new StringBuilder();
            if(Pass != null)
            {
                retBuilder.Append("Pass:" + Pass.ConvertToString() + ';');
            }
            retBuilder.Append("Database:" + Database + ';');
            retBuilder.Append("User:" + User + ';');
            retBuilder.Append("Host:" + Host + ";");
            return retBuilder.ToString();
        }

        public static DatabaseConfigurationFileContents Deserialize(string contentsIn)
        {
            DatabaseConfigurationFileContents ret = new DatabaseConfigurationFileContents();
            var contentSplit = contentsIn.Split(';');
            foreach(string field in contentSplit)
            {
                var fieldSplit = field.Split(':');
                
                if(fieldSplit[0].StartsWith("P"))
                {
                    ret.Pass = new SecureString();
                    foreach(char c in fieldSplit[1])
                    {
                        ret.Pass.AppendChar(c);
                    }
                } else if (fieldSplit[0].StartsWith("D"))
                {
                    ret.Database = fieldSplit[1];
                } else if (fieldSplit[0].StartsWith("U"))
                {
                    ret.User = fieldSplit[1];
                } else if (fieldSplit[0].StartsWith("H"))
                {
                    ret.Host = fieldSplit[1];
                }
            }
            return ret;
        }
    }

    static class DatabaseConfigurationFileHandler
    {
        public static string ConfigurationFileLocation = "./config.json";

        public static DatabaseConfigurationFileContents LoadConfigurationFile()
        {
            AnsBlockDecoder fileDecoder;
            fileDecoder = new AnsBlockDecoder(new MemoryStream());
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
                return DatabaseConfigurationFileContents.Deserialize(fileContents);
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
            string contents = fileContents.Serialize();
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

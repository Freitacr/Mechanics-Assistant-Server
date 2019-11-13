using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;

namespace OldManInTheShopServer.Util
{
    /// <summary>
    /// Helper class designed to turn working with the DataContractJsonSerializer object easier
    /// </summary>
    /// <typeparam name="T">Generic type that has the DataContract attribute applied to its definition.</typeparam>
    static class JsonDataObjectUtil<T>
    {
        public static T ParseObject(HttpListenerContext ctx)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            T req;
            try
            {
                req = (T)serializer.ReadObject(ctx.Request.InputStream);
            }
            catch (SerializationException)
            {
                return default;
            }
            return req;
        }

        public static T ParseObject(string source)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            T req;
            byte[] sourceBytes = Encoding.UTF8.GetBytes(source);
            MemoryStream stream = new MemoryStream(sourceBytes);
            try
            {
                req = (T)serializer.ReadObject(stream);
            }
            catch (SerializationException)
            {
                return default;
            }
            return req;
        }

        
        public static string ConvertObject(T source)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream streamOut = new MemoryStream();
            try
            {
                serializer.WriteObject(streamOut, source);
            } catch (SerializationException)
            {
                return null;
            }
            byte[] sourceBytes = streamOut.ToArray();
            return Encoding.UTF8.GetString(sourceBytes);
        }
    }
}

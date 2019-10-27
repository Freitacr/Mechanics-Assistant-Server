using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;

namespace OldManInTheShopServer.Util
{
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
    }
}

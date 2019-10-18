using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;

namespace MechanicsAssistantServer.Util
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
    }
}

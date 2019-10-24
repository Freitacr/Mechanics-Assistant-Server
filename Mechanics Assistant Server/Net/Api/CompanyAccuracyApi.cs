using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Net;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Net.Api
{
    [DataContract]
    class CompanyAccuracyApiFullPostRequest
    {
        [DataMember]
        public int UserId;
        [DataMember]
        public string LoginToken;
    }
    class CompanyAccuracyApi : ApiDefinition
    {
#if RELEASE
        public CompanyAccuracyApi(int port) : base("https://+:" + port + "/company/parts")
#elif DEBUG
        public CompanyAccuracyApi(int port) : base("http://+:" + port + "/company/parts")
#endif
        {
            GET += HandleGetRequest;
        }
        private void HandleGetRequest(HttpListenerContext ctx)
        {

        }
    }
}

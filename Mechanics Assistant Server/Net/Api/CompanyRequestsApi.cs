using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Net;
using OldManinTheShopServer.Data.MySql.TableDataTypes;
using OldManinTheShopServer.Data.MySql;
using OldManinTheShopServer.Util;

namespace OldManinTheShopServer.Net.Api
{
    [DataContract]
    class CompanyRequestsApiFullPostRequest
    {
        [DataMember]
        public int UserId;
        [DataMember]
        public string LoginToken;
        [DataMember]
        public string AuthToken;
        [DataMember]
        public int CompanyId;
    }
    class CompanyRequestsApi : ApiDefinition
    {
#if RELEASE
        public CompanyRequestsApi(int port) : base("https://+:" + port + "/company/parts")
#elif DEBUG
        public CompanyRequestsApi(int port) : base("http://+:" + port + "/company/parts")
#endif
        {
            POST += HandlePostRequest;
            GET += HandleGetRequest;
            DELETE += HandleDeleteRequest;
            PUT += HandlePutRequest;
        }
        private void HandlePostRequest(HttpListenerContext ctx)
        {

        }

        private void HandleGetRequest(HttpListenerContext ctx)
        {

        }

        private void HandleDeleteRequest(HttpListenerContext ctx)
        {

        }
        private void HandlePutRequest(HttpListenerContext ctx)
        {

        }
    }
}

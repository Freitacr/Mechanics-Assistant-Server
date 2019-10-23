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
    class CompanyPartsApiFullPostRequest
    {
        [DataMember]
        public int UserId;
        [DataMember]
        public string LoginToken;
        [DataMember]
        public string AuthToken;
        [DataMember]
        public int Make;
        [DataMember]
        public int Model;
        [DataMember]
        public int Year;
        [DataMember]
        public int PartId;
        [DataMember]
        public int PartName;
    }
    class CompanyPartsApi : ApiDefinition
    {
#if RELEASE
        public CompanyPartsApi(int port) : base("https://+:" + port + "/company/parts")
#elif DEBUG
        public CompanyPartsApi(int port) : base("http://+:" + port + "/company/parts")
#endif
        {
            POST += HandlePostRequest;
            GET += HandleGetRequest;
            DELETE += HandleDeleteRequest;
            PATCH += HandlePatchRequest;
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

        private void HandlePatchRequest(HttpListenerContext ctx)
        {

        }
    }
}

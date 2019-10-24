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
    class CompanyPartsRequestApiFullPostRequest
    {
        [DataMember]
        public int UserId;
        [DataMember]
        public string LoginToken;
        [DataMember]
        public string AuthToken;
        [DataMember]
        public int RepairJobId;
        [DataMember]
        public int CompanyId;
        [DataMember]
        public string PartsList;
        [DataMember]
        public int PartId;
        [DataMember]
        public int PartName;
    }
    class CompanyPartsRequestApi : ApiDefinition
    {
#if RELEASE
        public CompanyPartsRequestApi(int port) : base("https://+:" + port + "/company/parts")
#elif DEBUG
        public CompanyPartsRequestApi(int port) : base("http://+:" + port + "/company/parts")
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

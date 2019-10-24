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
    class CompanySettingsApiFullPostRequest
    {
        [DataMember]
        public int UserId;
        [DataMember]
        public string LoginToken;
        [DataMember]
        public string AuthToken;
        [DataMember]
        public string SettingsValue;
        [DataMember]
        public string SettingsKey;
    }
    class CompanySettingsApi : ApiDefinition
    {
#if RELEASE
        public CompanySettingsApi(int port) : base("https://+:" + port + "/company/parts")
#elif DEBUG
        public CompanySettingsApi(int port) : base("http://+:" + port + "/company/parts")
#endif
        {
            GET += HandleGetRequest;
            PATCH += HandlePatchRequest;
        }
        private void HandleGetRequest(HttpListenerContext ctx)
        {

        }
        private void HandlePatchRequest(HttpListenerContext ctx)
        {

        }
    }
}

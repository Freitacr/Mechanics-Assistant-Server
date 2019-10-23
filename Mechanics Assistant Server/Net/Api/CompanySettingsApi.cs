﻿using System;
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

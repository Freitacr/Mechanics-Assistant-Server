using System;
using System.Collections.Generic;
using System.Net;

namespace MechanicsAssistantServer.Net.Api
{
    public class ApiDefinition
    {
        protected string Uri;
        public UriMappingCollection ContainedDefinition { get; private set; }
        public ApiDefinition(string baseUri)
        {
            ContainedDefinition = new UriMappingCollection();
            Uri = baseUri;
        }

        protected bool AddAction(string method, Action<HttpListenerContext> action)
        {
            return ContainedDefinition.AddMapping(method, Uri, action);
        }
    }
}

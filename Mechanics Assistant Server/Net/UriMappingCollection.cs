using System;
using System.Collections.Generic;
using System.Net;
using OldManInTheShopServer.Data;
using OldManInTheShopServer.Net.Api;

namespace OldManInTheShopServer.Net
{
    public class UriMappingCollection
    {
        private List<ApiDefinition> ContainedCollection;
        private HashSet<HttpUri> Prefixes;

        public UriMappingCollection()
        {
            ContainedCollection = new List<ApiDefinition>();
            Prefixes = new HashSet<HttpUri>();
        }

        public bool AddMapping(ApiDefinition action)
        {
            if (Prefixes.Contains(action.URI))
                return false;
            ContainedCollection.Add(action);
            Prefixes.Add(action.URI);
            return true;
        }

        public IEnumerator<ApiDefinition> GetEnumerator()
        {
            return ContainedCollection.GetEnumerator();
        }

        public void AddPrefixes(HttpListener listener)
        {
            foreach (HttpUri prefix in Prefixes)
            {
                listener.Prefixes.Add(prefix.Prefix);
                Console.WriteLine("Registering prefix: " + prefix.Prefix);
            }
        }

        private ApiDefinition GetApi(string uri)
        {
            HttpUri uriIn = new HttpUri(uri);
            foreach (ApiDefinition def in this)
                if (def.URI.IsPrefixOf(uriIn))
                    return def;
            return null;
        }

        public ApiDefinition this[string x] {
            get => GetApi(x);
        }
    }
}

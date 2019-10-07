using System;
using System.Collections.Generic;
using System.Net;
using MechanicsAssistantServer.Data;
using MechanicsAssistantServer.Net.Api;

namespace MechanicsAssistantServer.Net
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

    public class UriMapping
    {
        public string Method { get; private set; }
        public string Uri { get; private set; }
        
        public UriMapping(string method, string uri)
        {
            Method = method ?? throw new ArgumentNullException("method");
            Uri = uri ?? throw new ArgumentNullException("uri");
            Method = Method.ToUpper();
            if (!Uri.EndsWith('/'))
                Uri += "/";
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != this.GetType())
                return false;
            UriMapping other = obj as UriMapping;
            return other.Method == Method && other.Uri == Uri;
        }

        public override int GetHashCode()
        {
            return Method.GetHashCode() + Uri.GetHashCode();
        }
    }
}

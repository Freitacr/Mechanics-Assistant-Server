using System;
using System.Collections.Generic;
using System.Net;

namespace MechanicsAssistantServer.Net
{
    public class UriMappingCollection
    {
        private Dictionary<UriMapping, Action<HttpListenerContext>> ContainedCollection;

        public UriMappingCollection()
        {
            ContainedCollection = new Dictionary<UriMapping, Action<HttpListenerContext>>();
        }

        public bool AddMapping(string method, string uri, Action<HttpListenerContext> action)
        {
            UriMapping mapping = new UriMapping(method, uri);
            if (ContainedCollection.ContainsKey(mapping))
                return false;
            ContainedCollection[mapping] = action;
            return true;
        }

        public void AddMappings(Api.ApiDefinition defIn)
        {
            AddMappings(defIn.ContainedDefinition);
        }

        public void AddMappings(UriMappingCollection collectionIn)
        {
            foreach (KeyValuePair<UriMapping, Action<HttpListenerContext>> mapping in collectionIn)
                ContainedCollection.Add(mapping.Key, mapping.Value);
        }

        public IEnumerator<KeyValuePair<UriMapping, Action<HttpListenerContext>>> GetEnumerator()
        {
            return ContainedCollection.GetEnumerator();
        }

        public Action<HttpListenerContext> this[UriMapping x] {
            get => ContainedCollection.GetValueOrDefault(x, null);
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

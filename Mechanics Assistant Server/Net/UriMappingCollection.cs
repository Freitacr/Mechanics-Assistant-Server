using System;
using System.Collections.Generic;
using System.Net;
using MechanicsAssistantServer.Net.Api;

namespace MechanicsAssistantServer.Net
{
    public class UriMappingCollection
    {
        private Dictionary<string, ApiDefinition> ContainedCollection;

        public UriMappingCollection()
        {
            ContainedCollection = new Dictionary<string, ApiDefinition>();
        }

        public bool AddMapping(ApiDefinition action)
        {
            if (ContainedCollection.ContainsKey(action.URI))
                return false;
            ContainedCollection[action.URI] = action;
            return true;
        }

        public IEnumerator<KeyValuePair<string, ApiDefinition>> GetEnumerator()
        {
            return ContainedCollection.GetEnumerator();
        }

        public ApiDefinition this[string x] {
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

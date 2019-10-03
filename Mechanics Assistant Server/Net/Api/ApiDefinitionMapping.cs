using System;
using System.Collections.Generic;
using System.Text;

namespace MechanicsAssistantServer.Net.Api
{
    public class ApiDefinitionMapping
    {
        private readonly Dictionary<string, ApiDefinition> MappedDefinitions;

        public ApiDefinitionMapping()
        {
            MappedDefinitions = new Dictionary<string, ApiDefinition>();
        }

        public bool AddDefinition(ApiDefinition definition)
        {
            if (MappedDefinitions.ContainsKey(definition.URI))
                return false;
            MappedDefinitions[definition.URI] = definition;
            return true;
        }

        public ApiDefinition RetrieveDefinition(string uri)
        {
            if (!MappedDefinitions.ContainsKey(uri))
                return null;
            return MappedDefinitions[uri];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using MechanicsAssistantServer.Models;

namespace MechanicsAssistantServer.Net.Api
{
    public static class ApiLoader
    {

        public static QueryResponseServer LoadApiAndListen(int portIn, QueryProcessor processorIn)
        {
            QueryResponseServer ret = new QueryResponseServer();
            UriMappingCollection api = new UriMappingCollection();
            api.AddMappings(new TopLevelApi(portIn, ret));
            api.AddMappings(new QueryLevelApi(portIn, processorIn));
            ret.ListenForResponses(api);
            return ret;
        }
    }
}

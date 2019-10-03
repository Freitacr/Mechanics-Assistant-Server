using System.Reflection;
using System.Linq;
using MechanicsAssistantServer.Models;

namespace MechanicsAssistantServer.Net.Api
{
    public static class ApiLoader
    {

        public static QueryResponseServer LoadApiAndListen(int portIn, QueryProcessor processorIn)
        {
            QueryResponseServer ret = new QueryResponseServer();
            UriMappingCollection api = new UriMappingCollection();
            api.AddMapping(new TopLevelApi(portIn, ret));
            api.AddMapping(new QueryLevelApi(portIn, processorIn));
            ret.ListenForResponses(api);
            return ret;
        }
    }
}

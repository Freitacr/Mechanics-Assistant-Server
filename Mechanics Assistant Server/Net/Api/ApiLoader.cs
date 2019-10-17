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
            api.AddMapping(new QueryLevelApi(portIn, processorIn));
            api.AddMapping(new CertValidationApi());
            api.AddMapping(new TopLevelApi());
            api.AddMapping(new RepairJobApi(portIn));
            api.AddMapping(new UserApi(portIn));
            ret.ListenForResponses(api);
            return ret;
        }
    }
}

using System.Reflection;
using System.Linq;
using OldManInTheShopServer.Models;

namespace OldManInTheShopServer.Net.Api
{
    public static class ApiLoader
    {

        public static QueryResponseServer LoadApiAndListen(int portIn)
        {
            UriMappingCollection api = new UriMappingCollection();
            QueryResponseServer ret = new QueryResponseServer();
            api.AddMapping(new CertValidationApi());
            api.AddMapping(new TopLevelApi());
            api.AddMapping(new RepairJobRequirementApi(portIn));
            api.AddMapping(new RepairJobReportApi(portIn));
            api.AddMapping(new RepairJobApi(portIn));
            api.AddMapping(new UserAuthApi(portIn));
            api.AddMapping(new UserSettingsApi(portIn));
            api.AddMapping(new ReportUserApi(portIn));
            api.AddMapping(new UserJobApi(portIn));
            api.AddMapping(new UserRequestsApi(portIn));
            api.AddMapping(new UserApi(portIn));
            api.AddMapping(new CompanyAccuracyApi(portIn));
            api.AddMapping(new CompanyPartsRequestApi(portIn));
            api.AddMapping(new CompanyPartsApi(portIn));
            api.AddMapping(new CompanyForumApi(portIn));
            api.AddMapping(new CompanySafetyRequestApi(portIn));
            api.AddMapping(new CompanySettingsApi(portIn));
            api.AddMapping(new CompanyPartslistsRequestsApi(portIn));
            api.AddMapping(new CompanyRequestsApi(portIn));
            api.AddMapping(new PredictApi(portIn));
            ret.ListenForResponses(api);
            return ret;
        }
    }
}

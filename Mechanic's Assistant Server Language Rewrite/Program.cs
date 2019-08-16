using System;
using System.Net;
using System.Collections.Generic;
using MechanicsAssistantServer.Net;

namespace MechanicsAssistantServer
{
    class ProgramMain
    {

        public static void HandleTestPut(HttpListenerContext ctx)
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.StatusDescription = "Ok";
            ctx.Response.OutputStream.Close();
        }

        static void Main(string[] args)
        {
            UriMappingCollection mappingCollection = new UriMappingCollection();
            mappingCollection.AddMapping("put", "http://localhost:16384/testput", HandleTestPut);
            QueryResponseServer server = new QueryResponseServer(mappingCollection);
            server.ListenForResponses();
            Console.WriteLine("Simple server started... press any key to exit");
            Console.ReadKey();
            server.Close();
            //QueryProcessor processor = new QueryProcessor(QueryProcessorSettings.GenerateDefaultSettings());
            //processor.ProcessQuery(new Util.MechanicQuery("autocar", "xpeditor", null, null, "runs rough"));
        }
    }
}

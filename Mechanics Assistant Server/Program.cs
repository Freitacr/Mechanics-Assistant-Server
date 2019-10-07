using System.Net;
using System.IO;
using System.Threading;
using MechanicsAssistantServer.Net.Api;
using MechanicsAssistantServer.Models;
using System;

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
            var server = ApiLoader.LoadApiAndListen(16384, new QueryProcessor(QueryProcessorSettings.GenerateDefaultSettings()));
            while (server.IsAlive)
                Thread.Sleep(100);
            //QueryProcessor processor = new QueryProcessor(QueryProcessorSettings.GenerateDefaultSettings());
            //processor.ProcessQuery(new Util.MechanicQuery("autocar", "xpeditor", null, null, "runs rough"));
        }
    }
}

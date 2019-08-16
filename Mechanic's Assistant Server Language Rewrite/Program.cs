using System;
using System.IO;
using System.Collections.Generic;
using MechanicsAssistantServer.Models;
using MechanicsAssistantServer.Models.POSTagger;
using MechanicsAssistantServer.Models.KeywordPrediction;


namespace MechanicsAssistantServer
{
    class ProgramMain
    {

        static void Main(string[] args)
        {
            QueryProcessor processor = new QueryProcessor(QueryProcessorSettings.GenerateDefaultSettings());
            processor.ProcessQuery(new Util.MechanicQuery("autocar", "xpeditor", null, null, "runs rough"));
        }
    }
}

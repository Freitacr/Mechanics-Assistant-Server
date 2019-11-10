using System.Threading;
using OldManInTheShopServer.Net.Api;
using CertesWrapper;
using System;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Models.POSTagger;

namespace OldManInTheShopServer
{
    class ProgramMain
    {

        static void RenewCertificate()
        {
            while (true)
            {
                if (CertificateRenewer.CertificateNeedsRenewal())
                    CertificateRenewer.GetFirstCert(false);
                try
                {
                    Thread.Sleep(TimeSpan.FromMinutes(30));
                } catch (ThreadInterruptedException)
                {
                    Console.WriteLine("Certificate Renewal Thread Exiting");
                    break;
                }
            }
        }

        static DatabaseConfigurationFileContents RetrieveConfiguration(CommandLineArgumentParser parser)
        {
            DatabaseConfigurationFileContents config = DatabaseConfigurationFileHandler.LoadConfigurationFile();
            if(config == null)
            {
                if (!DatabaseConfigurationFileHandler.WriteConfigurationFileDefaults())
                    return null;
                config = DatabaseConfigurationFileHandler.LoadConfigurationFile();
                if (!parser.KeyedArguments.ContainsKey("-p"))
                    return null;
                config.Pass = parser.KeyedArguments["-p"];
            }
            return config;
        }

        static void Main(string[] args)
        {
            CommandLineArgumentParser parser = new CommandLineArgumentParser(args);
            DatabaseConfigurationFileContents config = RetrieveConfiguration(parser);
            if(config == null)
            {
                Console.WriteLine("Failed to retrieve or restore database configuration file. Exiting");
                return;
            }
            if(config.Pass == null)
            {
                Console.WriteLine("Configuration did not contain a password, this is an irrecoverable error. Exiting");
                return;
            }

            bool res = MySqlDataManipulator.GlobalConfiguration.Connect(new MySqlConnectionString(config.Host, config.Database, config.User).ConstructConnectionString(config.Pass));
            
            if(!res && MySqlDataManipulator.GlobalConfiguration.LastException.Number != 1049 && MySqlDataManipulator.GlobalConfiguration.LastException.Number != 0)
            {
                Console.WriteLine("Encountered an error opening the global configuration connection");
                Console.WriteLine(MySqlDataManipulator.GlobalConfiguration.LastException.Message);
                return;
            }
            if(!MySqlDataManipulator.GlobalConfiguration.ValidateDatabaseIntegrity(config.Database))
            {
                Console.WriteLine("Encountered an error opening the global configuration connection");
                Console.WriteLine(MySqlDataManipulator.GlobalConfiguration.LastException.Message);
                return;
            }
            MySqlDataManipulator.GlobalConfiguration.Close();
            MySqlDataManipulator.GlobalConfiguration.Connect(new MySqlConnectionString(config.Host, config.Database, config.User).ConstructConnectionString(config.Pass));
            bool exit = DatabaseEntityCreationUtilities.PerformRequestedCreation(MySqlDataManipulator.GlobalConfiguration, parser);
            MySqlDataManipulator.GlobalConfiguration.Close();
            if (exit)
                return;
            if (!GlobalModelHelper.LoadOrTrainGlobalModels(ReflectionHelper.GetAllKeywordPredictors()))
                throw new NullReferenceException("One or more global models failed to load. Server cannot start.");
            else if(AveragedPerceptronTagger.GetTagger() == null)
                throw new NullReferenceException("Failed to load the Averaged Perceptron Tagger");
            Logger.Global.Log(Logger.LogLevel.INFO, "Server is starting up");
            Thread t = new Thread(RenewCertificate);
            t.Start();
            var server = ApiLoader.LoadApiAndListen(16384);
            while (server.IsAlive)
            {
                Thread.Sleep(100);
                if(Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                        server.Close();
                }
            }
            t.Interrupt();
            //QueryProcessor processor = new QueryProcessor(QueryProcessorSettings.GenerateDefaultSettings());
            //processor.ProcessQuery(new Util.MechanicQuery("autocar", "xpeditor", null, null, "runs rough"));
        }
    }
}

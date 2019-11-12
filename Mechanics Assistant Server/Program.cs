using System.Threading;
using OldManInTheShopServer.Net.Api;
using CertesWrapper;
using System;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;
using MySql.Data.MySqlClient;
using OldManInTheShopServer.Models.POSTagger;
using System.Security;

namespace OldManInTheShopServer
{
    class ProgramMain
    {

        static void RenewCertificate()
        {
            try
            {
                Thread.Sleep(TimeSpan.FromMinutes(1));
                while (true)
                {
                    if (CertificateRenewer.CertificateNeedsRenewal())
                    {
                        CertificateRenewer.GetFirstCert(false);
                    }
                    Thread.Sleep(TimeSpan.FromMinutes(30));
                }
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine("Certificate Renewal Thread Exiting");
            }
        }

        static DatabaseConfigurationFileContents RetrieveConfiguration()
        {
            DatabaseConfigurationFileContents config = DatabaseConfigurationFileHandler.LoadConfigurationFile();
            if(config == null)
            {
                config = DatabaseConfigurationFileHandler.GenerateDefaultConfiguration();
                SecureString password = MaskedPasswordReader.ReadPasswordMasked("Please enter the password for the default MySql user");
                config.Pass = password;
                if (!DatabaseConfigurationFileHandler.WriteConfigurationFile(config))
                    return null;
            }
            return config;
        }

        static void Main(string[] args)
        {
            DatabaseConfigurationFileContents config;
            try
            {
                config = RetrieveConfiguration();
            } catch (ThreadInterruptedException)
            {
                return;
            }
            if(config == null)
            {
                Console.WriteLine("Failed to retrieve or restore database configuration file. Exiting");
                return;
            }
            bool res = MySqlDataManipulator.GlobalConfiguration.Connect(new MySqlConnectionString(config.Host, config.Database, config.User).ConstructConnectionString(config.Pass.ConvertToString()));
            
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
            CommandLineArgumentParser parser = new CommandLineArgumentParser(args);
            MySqlDataManipulator.GlobalConfiguration.Connect(new MySqlConnectionString(config.Host, config.Database, config.User).ConstructConnectionString(config.Pass.ConvertToString()));
            config.Pass.Dispose();
            config = null;
            bool exit = DatabaseEntityCreationUtilities.PerformRequestedCreation(MySqlDataManipulator.GlobalConfiguration, parser);
            MySqlDataManipulator.GlobalConfiguration.Close();
            if (exit)
                return;
            if (!GlobalModelHelper.LoadOrTrainGlobalModels(ReflectionHelper.GetAllKeywordPredictors()))
                throw new NullReferenceException("One or more global models failed to load. Server cannot start.");
            else if(AveragedPerceptronTagger.GetTagger() == null)
                throw new NullReferenceException("Failed to load the Averaged Perceptron Tagger");
            Logger.Global.Log(Logger.LogLevel.INFO, "Server is starting up");
            using (Logger.Global)
            {
                Thread t = new Thread(RenewCertificate);
                t.Start();
                var server = ApiLoader.LoadApiAndListen(16384);
                while (server.IsAlive)
                {
                    Thread.Sleep(100);
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Enter)
                            server.Close();
                    }
                }
                t.Interrupt();
            }
            //QueryProcessor processor = new QueryProcessor(QueryProcessorSettings.GenerateDefaultSettings());
            //processor.ProcessQuery(new Util.MechanicQuery("autocar", "xpeditor", null, null, "runs rough"));
        }
    }
}

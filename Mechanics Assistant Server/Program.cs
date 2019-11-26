using System.Threading;
using OldManInTheShopServer.Net.Api;
using CertesWrapper;
using System;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;
using System.Linq;
using OldManInTheShopServer.Models.POSTagger;
using System.Security;
using System.Collections.Generic;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Models;

namespace OldManInTheShopServer
{
    class ProgramMain
    {
        

        static void PerformTraining()
        {
            try
            {
                while (true)
                {
                    MySqlDataManipulator manipulator = new MySqlDataManipulator();
                    if (!manipulator.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString())){
                        throw new ArgumentException("MySqlDataManipulator failed to connect to the database");
                    }
                    Console.WriteLine("Checking company training statuses");
                    List<CompanyId> companies = manipulator.GetCompaniesWithNamePortion("");
                    foreach(CompanyId company in companies)
                    {
                        if (manipulator.GetCountInTable(TableNameStorage.CompanyValidatedRepairJobTable.Replace("(n)", company.Id.ToString())) != 0)
                        {
                            DateTime lastTrainedTime = DateTime.Parse(company.LastTrainedTime);
                            CompanySettingsEntry trainInterval = manipulator.GetCompanySettingsWhere(company.Id, "SettingKey=\"" + CompanySettingsKey.RetrainInterval + "\"")[0];
                            bool shouldTrain = lastTrainedTime.AddDays(int.Parse(trainInterval.SettingValue)) <= DateTime.Now;
                            if (shouldTrain)
                            {
                                Console.WriteLine("Performing training for company " + company.LegalName);
                                DatabaseQueryProcessor processor = new DatabaseQueryProcessor(DatabaseQueryProcessorSettings.RetrieveCompanySettings(manipulator, company.Id));
                                CompanyModelUtils.TrainClusteringModel(manipulator, processor, company.Id, training: false);
                                company.LastTrainedTime = DateTime.Now.ToString();
                                manipulator.UpdateCompanyTrainingTime(company);
                                double automatedTestingResults = CompanyModelUtils.PerformAutomatedTesting(manipulator, company.Id, processor);
                                company.ModelAccuracy = (float)(100-automatedTestingResults);
                                manipulator.UpdateCompanyAutomatedTestingResults(company);
                                Console.WriteLine("Accuracy after training: " + company.ModelAccuracy);
                            }
                        }
                        if (manipulator.GetCountInTable(TableNameStorage.CompanyNonValidatedRepairJobTable.Replace("(n)", company.Id.ToString())) != 0)
                        {
                            DateTime lastValidatedTime = DateTime.Parse(company.LastValidatedTime);
                            bool shouldValidate = lastValidatedTime.AddDays(14) <= DateTime.Now;
                            if(shouldValidate)
                            {
                                Console.WriteLine("Attempting to validate some non-validated data for company " + company.LegalName);
                                DatabaseQueryProcessor processor = new DatabaseQueryProcessor(DatabaseQueryProcessorSettings.RetrieveCompanySettings(manipulator, company.Id));
                                CompanyModelUtils.PerformDataValidation(manipulator, company.Id, processor);
                            }
                        }
                    }
                    manipulator.Close();
                    Thread.Sleep(TimeSpan.FromMinutes(120));
                }
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine("Retraining Thread Exiting");
            }
        }

        static void RenewCertificate()
        {
            try
            {
                Thread.Sleep(TimeSpan.FromMinutes(1));
                while (true)
                {
                    Console.WriteLine("Checking certificate status");
                    if (CertificateRenewer.CertificateNeedsRenewal())
                    {
                        Console.WriteLine("Attempting to retrieve new certificate");
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
            Console.WriteLine(DateTime.Now.ToLocalTime().ToString());
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
            if(!MySqlDataManipulator.GlobalConfiguration.ValidateDatabaseIntegrity(new MySqlConnectionString(config.Host, null, config.User).ConstructConnectionString(config.Pass.ConvertToString()), config.Database))
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
            Logger.GetLogger(Logger.LoggerDefaultFileLocations.DEFAULT).Log(Logger.LogLevel.INFO, "Server is starting up");
            using (Logger.Disposer)
            {
                Thread t = new Thread(RenewCertificate);
                t.Start();
                Thread train = new Thread(PerformTraining);
                train.Start();
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
                train.Interrupt();
            }
            //QueryProcessor processor = new QueryProcessor(QueryProcessorSettings.GenerateDefaultSettings());
            //processor.ProcessQuery(new Util.MechanicQuery("autocar", "xpeditor", null, null, "runs rough"));
        }
    }
}

using System.Threading;
using OldManinTheShopServer.Net.Api;
using OldManinTheShopServer.Models;
using CertesWrapper;
using System;
using OldManinTheShopServer.Data.MySql;
using OldManinTheShopServer.Util;

namespace OldManinTheShopServer
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

        static void Main(string[] args)
        {
            bool res = MySqlDataManipulator.GlobalConfiguration.Connect(new MySqlConnectionString("localhost", "db_test", "testUser").ConstructConnectionString(""));
            if(!res && MySqlDataManipulator.GlobalConfiguration.LastException.Number != 1049)
            {
                Console.WriteLine("Encountered an error opening the global configuration connection");
                Console.WriteLine(MySqlDataManipulator.GlobalConfiguration.LastException.Message);
                return;
            }
            if(!MySqlDataManipulator.GlobalConfiguration.ValidateDatabaseIntegrity("db_test"))
            {
                Console.WriteLine("Encountered an error opening the global configuration connection");
                Console.WriteLine(MySqlDataManipulator.GlobalConfiguration.LastException.Message);
                return;
            }
            MySqlDataManipulator.GlobalConfiguration.Close();
            MySqlDataManipulator.GlobalConfiguration.Connect(new MySqlConnectionString("localhost", "db_test", "testUser").ConstructConnectionString(""));
            CommandLineArgumentParser parser = new CommandLineArgumentParser(args);
            if(parser.KeyedArguments.ContainsKey("-c"))
            {
                if(parser.KeyedArguments["-c"].Equals("company"))
                {
                    if(!MySqlDataManipulator.GlobalConfiguration.AddCompany(string.Join(" ", parser.PositionalArguments)))
                    {
                        Console.WriteLine("Failed to add company " + string.Join(" ", parser.PositionalArguments));
                        Console.WriteLine("Failed because of error " + MySqlDataManipulator.GlobalConfiguration.LastException.Message);
                        MySqlDataManipulator.GlobalConfiguration.Close();
                        return;
                    }
                    Console.WriteLine("Successfully added company " + string.Join(" ", parser.PositionalArguments));
                    MySqlDataManipulator.GlobalConfiguration.Close();
                    return;
                } else
                {
                    Console.WriteLine("Only company creation is supported. Use -c company.");
                    MySqlDataManipulator.GlobalConfiguration.Close();
                    return;
                }
            }
            MySqlDataManipulator.GlobalConfiguration.Close();
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

using System.Threading;
using MechanicsAssistantServer.Net.Api;
using MechanicsAssistantServer.Models;
using CertesWrapper;
using System;
using MechanicsAssistantServer.Data.MySql;

namespace MechanicsAssistantServer
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
            Thread t = new Thread(RenewCertificate);
            t.Start();
            bool res = MySqlDataManipulator.GlobalConfiguration.Connect(new MySqlConnectionString("localhost", "db_test", "testUser").ConstructConnectionString(""));
            if(!res && MySqlDataManipulator.GlobalConfiguration.LastException.Number != 1049)
            {
                Console.WriteLine("Encountered an error opening the global configuration connection");
                Console.WriteLine(MySqlDataManipulator.GlobalConfiguration.LastException.Message);
                t.Interrupt();
                return;
            }
            if(!MySqlDataManipulator.GlobalConfiguration.ValidateDatabaseIntegrity("db_test"))
            {
                Console.WriteLine("Encountered an error opening the global configuration connection");
                Console.WriteLine(MySqlDataManipulator.GlobalConfiguration.LastException.Message);
                t.Interrupt();
                return;
            }
            MySqlDataManipulator.GlobalConfiguration.Close();
            MySqlDataManipulator.GlobalConfiguration.Connect(new MySqlConnectionString("localhost", "db_test", "testUser").ConstructConnectionString(""));
            MySqlDataManipulator.GlobalConfiguration.Close();
            var server = ApiLoader.LoadApiAndListen(16384, new QueryProcessor(QueryProcessorSettings.GenerateDefaultSettings()));
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

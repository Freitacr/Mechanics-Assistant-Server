using System.Net;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using MechanicsAssistantServer.Net.Api;
using MechanicsAssistantServer.Models;
using CertesWrapper;
using System;
using MySql.Data;
using MySql.Data.MySqlClient;

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

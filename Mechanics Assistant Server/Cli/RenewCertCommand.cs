using System;
using System.Collections.Generic;
using System.Text;
using CertesWrapper;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Net.Api;

namespace OldManInTheShopServer.Cli
{
    class RenewCertCommand : CommandLineCommand
    {
        /// <summary>
        /// <para>Flag to differentiate this command from other command line commands</para>
        /// </summary>
        [KeyedArgument("-rc")]
        public string Flag = default;

        /// <summary>
        /// Function to renew HTTPS certification.
        /// </summary>
        /// <param name="manipulator"><see cref="MySqlDataManipulator"/> used to add the company to the database</param>
        /// <remarks>As this is a command to be used by the developers on the project, error output is minimal</remarks>
        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            var server = ApiLoader.LoadApiAndListen(16384);
            Console.WriteLine("Attempting to retrieve new certificate");
            CertificateRenewer.GetFirstCert(false);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using Certes;
using Certes.Acme;
using Certes.Acme.Resource;

namespace CertesWrapper
{
    static class AccountHelper
    {
        public static IAccountContext RetriveAccount(AcmeContext acme)
        {
            return acme.Account().Result;
        }

        public static IAccountContext GenerateAccount(AcmeContext acme, string email)
        {
            return acme.NewAccount(email, true).Result;
        }

        public static Account RetriveAccountDetails(IAccountContext ctx)
        {
            return ctx.Resource().Result;
        }

        public static AcmeContext GetContextWithAccount(string keyFileLocation, Uri server)
        {
            StreamReader reader = new StreamReader(keyFileLocation);
            var readText = reader.ReadToEnd();
            var accountKey = KeyFactory.FromPem(readText);
            return new AcmeContext(server, accountKey);
        }
    }
}

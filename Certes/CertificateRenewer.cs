using System;
using Certes.Acme;
using Certes;
using Certes.Acme.Resource;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CertesWrapper
{
    class ContextAccountBundle
    {
        public AcmeContext Ctx { get; private set; }
        public IAccountContext Account { get; private set; }

        public ContextAccountBundle(AcmeContext ctx, IAccountContext account)
        {
            Ctx = ctx;
            Account = account;
        }
    }

    public static class CertificateRenewer
    {

        private static readonly string STAGING_ACC_LOC = "pemkey.key";
        private static readonly string ACC_LOC = "accountKey.key";

        public static bool CertificateNeedsRenewal()
        {
            ProcessStartInfo psi = new ProcessStartInfo("powershell", "-Command ./countExpiring.ps1");
            psi.CreateNoWindow = true;
            var countProcess = Process.Start(psi);
            countProcess.WaitForExit();
            if (countProcess.ExitCode > 0)
                return true;
            return false;
        }


        static async Task<IAccountContext> RetrieveAccount(AcmeContext acme)
        {
            return await acme.Account();
        }

        public static void GetFirstCert(bool staging = true)
        {
            ContextAccountBundle bundle;
            if (staging)
            {
                bundle = GetStagingParameters();
            }
            else
            {
                bundle = GetNonStagingParameters();
            }

            var account = bundle.Account;
            var ctx = bundle.Ctx;

            IOrderListContext orders = account.Orders().Result;
            List<IOrderContext> orderContexts = new List<IOrderContext>(orders.Orders().Result);
            IOrderContext currentOrder = null;
            if (orderContexts.Count == 0)
            {
                currentOrder = ctx.NewOrder(new[] { "jcf-ai.com" }).Result;
            }
            else
            {
                foreach (IOrderContext orderCtx in orderContexts)
                {
                    if (orderCtx.Resource().Result.Status != OrderStatus.Valid)
                    {
                        currentOrder = orderCtx;
                        break;
                    }
                }
                if (currentOrder == null)
                {
                    currentOrder = ctx.NewOrder(new[] { "jcf-ai.com" }).Result;
                }
            }
            Order order = currentOrder.Resource().Result;
            var authorizationList = currentOrder.Authorizations().Result;
            IAuthorizationContext currentAuthContext;
            IEnumerator<IAuthorizationContext> enumerator = authorizationList.GetEnumerator();
            while (enumerator.Current == null)
                enumerator.MoveNext();
            currentAuthContext = enumerator.Current;
            Authorization authResource = currentAuthContext.Resource().Result;


            IChallengeContext httpContext = currentAuthContext.Http().Result;
            if (httpContext.Resource().Result.Status != ChallengeStatus.Valid)
            {
                StreamWriter writer = new StreamWriter(httpContext.Token);
                writer.Write(httpContext.KeyAuthz);
                writer.Close();
                Challenge httpChallenge = httpContext.Validate().Result;
                if (httpChallenge.Status.Value != ChallengeStatus.Valid)
                {
                    throw new AcmeException("Validation failed.");
                }
            }

            //Everything should be good to go on the whole validate everything front.
            IKey privateKey = KeyFactory.NewKey(KeyAlgorithm.ES384);
            var cert = currentOrder.Generate(
                new CsrInfo()
                {
                    CountryName = "US",
                    State = "Washington",
                    Locality = "Tacoma",
                    CommonName = "jcf-ai.com"
                }, privateKey).Result;

            byte[] pfx = cert.ToPfx(privateKey).Build("jcf-ai.com", "pass");
            FileStream pfxOutStream = new FileStream("jcf-ai.pfx", FileMode.Create, FileAccess.Write);
            pfxOutStream.Write(pfx, 0, pfx.Length);
            pfxOutStream.Close();

            ProcessStartInfo psi = new ProcessStartInfo("powershell", "-Command ./bindcert.ps1 jcf-ai.pfx pass");
            psi.CreateNoWindow = true;
            Process.Start(psi);
        }

        static ContextAccountBundle GetStagingParameters()
        {
            Uri server = WellKnownServers.LetsEncryptStagingV2;
            AcmeContext ctx;
            if (File.Exists(STAGING_ACC_LOC))
            {
                ctx = AccountHelper.GetContextWithAccount(STAGING_ACC_LOC, server);
            }
            else
            {
                ctx = new AcmeContext(server);
            }
            IAccountContext account = AccountHelper.RetriveAccount(ctx);
            Account accInfo = AccountHelper.RetriveAccountDetails(account);
            if (!accInfo.Status.HasValue)
                throw new AcmeException("Account has not had its status set yet");
            else if (accInfo.Status.Value == AccountStatus.Revoked || accInfo.Status.Value == AccountStatus.Deactivated)
                throw new AcmeException("Account is either revoked or deactivated");

            return new ContextAccountBundle(ctx, account);
        }

        static ContextAccountBundle GetNonStagingParameters()
        {
            Uri server = WellKnownServers.LetsEncryptV2;
            AcmeContext ctx;
            if (File.Exists(ACC_LOC))
            {
                ctx = AccountHelper.GetContextWithAccount(ACC_LOC, server);
            }
            else
            {
                ctx = new AcmeContext(server);
            }
            IAccountContext account = AccountHelper.RetriveAccount(ctx);
            Account accInfo = AccountHelper.RetriveAccountDetails(account);
            if (!accInfo.Status.HasValue)
                throw new AcmeException("Account has not had its status set yet");
            else if (accInfo.Status.Value == AccountStatus.Revoked || accInfo.Status.Value == AccountStatus.Deactivated)
                throw new AcmeException("Account is either revoked or deactivated");
            return new ContextAccountBundle(ctx, account);
        }

    }
}

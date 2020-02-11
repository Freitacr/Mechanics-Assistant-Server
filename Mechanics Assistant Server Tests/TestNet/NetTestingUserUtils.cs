using OldManInTheShopServer.Data.MySql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using OldManInTheShopServer.Net.Api;
using OldManInTheShopServer.Util;

namespace MechanicsAssistantServerTests.TestNet
{
    class NetTestingUserUtils
    {

        public static bool LogInTestingUser(TestingUserStorage.TestingUser userIn)
        {
            UserApi api = new UserApi(10000);
            object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                userIn.ConstructLoginRequest(),
                "PUT");
            var ctx = contextAndRequest[0] as HttpListenerContext;
            var req = contextAndRequest[1] as HttpWebRequest;
            api.PUT(ctx);
            HttpWebResponse resp;
            try
            {
                resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                return true;
            } catch
            {
                return false;
            }
        }

        public static bool AuthenticateTestingUser(TestingUserStorage.TestingUser userIn, MySqlDataManipulator manipulatorIn)
        {
            UserAuthApi api = new UserAuthApi(10000);
            if (!LogInTestingUser(userIn)) return false;

            var databaseUser = manipulatorIn.GetUsersWhere(string.Format("Email=\"{0}\"", userIn.Email))[0];
            var loginTokens = UserVerificationUtil.ExtractLoginTokens(databaseUser);
            object[] contextAndRequest = ServerTestingMessageSwitchback.SwitchbackMessage(
                userIn.ConstructAuthenticationRequest(loginTokens.LoginToken, databaseUser.UserId),
                "PUT");
            var ctx = contextAndRequest[0] as HttpListenerContext;
            var req = contextAndRequest[1] as HttpWebRequest;
            api.PUT(ctx);
            HttpWebResponse resp;
            try
            {
                resp = req.EndGetResponse(contextAndRequest[2] as IAsyncResult) as HttpWebResponse;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

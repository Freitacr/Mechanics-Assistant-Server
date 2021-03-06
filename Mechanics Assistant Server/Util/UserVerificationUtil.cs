﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Util
{
    /// <summary>
    /// Utility class to ease the testing of the validity of login and auth tokens and
    /// testing for a valid initial login attempt or intitial authentication attempt
    /// </summary>
    class UserVerificationUtil
    {
        public static bool LoginTokenValid(OverallUser databaseUser, string loginToken)
        {
            byte[] convertedText = Encoding.UTF8.GetBytes(databaseUser.LoginStatusTokens);
            DataContractJsonSerializer loggedTokenSerializer = new DataContractJsonSerializer(typeof(LoginStatusTokens));
            LoginStatusTokens dbTokens = loggedTokenSerializer.ReadObject(new MemoryStream(convertedText)) as LoginStatusTokens;
            if (dbTokens == null)
                throw new ArgumentException("database user had an invalid entry for logged tokens");
            if (!loginToken.Equals(dbTokens.LoginToken))
                return false;
            DateTime dbExpiration = DateTime.Parse(dbTokens.LoginTokenExpiration);
            return DateTime.UtcNow.CompareTo(dbExpiration) < 0;
        }

        public static bool AuthTokenValid(OverallUser databaseUser, string authToken)
        {
            byte[] convertedText = Encoding.UTF8.GetBytes(databaseUser.LoginStatusTokens);
            DataContractJsonSerializer loggedTokenSerializer = new DataContractJsonSerializer(typeof(LoginStatusTokens));
            LoginStatusTokens dbTokens = loggedTokenSerializer.ReadObject(new MemoryStream(convertedText)) as LoginStatusTokens;
            if (dbTokens == null)
                throw new ArgumentException("database user had an invalid entry for logged tokens");
            if (!authToken.Equals(dbTokens.AuthToken))
                return false;
            DateTime dbExpiration = DateTime.Parse(dbTokens.AuthTokenExpiration);
            return DateTime.UtcNow.CompareTo(dbExpiration) < 0;
        }

        public static bool VerifyLogin(OverallUser databaseUser, string email, string password)
        {
            byte[] calcToken = SecurityLibWrapper.SecLib.ConstructDerivedSecurityToken(Encoding.UTF8.GetBytes(email), Encoding.UTF8.GetBytes(password));
            byte[] dbToken = databaseUser.DerivedSecurityToken;
            for (int i = 0; i < dbToken.Length; i++)
                if (dbToken[i] != calcToken[i])
                    return false;
            return true;
        }

        public static bool VerifyAuthentication(OverallUser databaseUser, string securityQuestion, string securityAnswer)
        {
            if (!securityQuestion.Equals(databaseUser.SecurityQuestion))
                return false;
            byte[] calcKey = SecurityLibWrapper.SecLib.ConstructUserEncryptionKey(databaseUser.DerivedSecurityToken, Encoding.UTF8.GetBytes(securityAnswer));
            byte[] key = new byte[32];
            byte[] iv = new byte[16];
            for (int i = 0, j = 32; j < databaseUser.DerivedSecurityToken.Length; i++, j++)
            {
                key[i] = calcKey[i];
                if ((i & 1) == 0)
                    iv[i >> 1] = calcKey[j];
            }
            Aes aes = Aes.Create();
            string toTest;
            using (aes)
            {
                var decrypt = aes.CreateDecryptor(key, iv);
                try
                {
                    byte[] pass = decrypt.TransformFinalBlock(databaseUser.AuthTestString, 0, databaseUser.AuthTestString.Length);
                    toTest = Encoding.UTF8.GetString(pass);
                }
                catch (CryptographicException)
                {
                    return false;
                }
            }
            return toTest.Equals("pass");
        }

        public static LoginStatusTokens ExtractLoginTokens(OverallUser userIn)
        {
            string loggedTokensJson = userIn.LoginStatusTokens;
            loggedTokensJson = loggedTokensJson.Replace("\\\"", "\"");
            byte[] tokens = Encoding.UTF8.GetBytes(loggedTokensJson);
            MemoryStream stream = new MemoryStream(tokens);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LoginStatusTokens));
            LoginStatusTokens ret = serializer.ReadObject(stream) as LoginStatusTokens;
            return ret;
        }

        public static void GenerateNewLoginToken(LoginStatusTokens tokens)
        {
            Random rand = new Random();
            byte[] loginToken = new byte[64];
            rand.NextBytes(loginToken);
            tokens.LoginToken = MysqlDataConvertingUtil.ConvertToHexString(loginToken);
            DateTime now = DateTime.UtcNow;
            now = now.AddHours(3);
            tokens.LoginTokenExpiration = now.ToString();
        }

        public static void GenerateNewAuthToken(LoginStatusTokens tokens)
        {
            Random rand = new Random();
            byte[] loginToken = new byte[64];
            rand.NextBytes(loginToken);
            tokens.AuthToken = MysqlDataConvertingUtil.ConvertToHexString(loginToken);
            DateTime now = DateTime.UtcNow;
            now = now.AddHours(.5);
            tokens.AuthTokenExpiration = now.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using OldManinTheShopServer.Data.MySql.TableDataTypes;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using OldManinTheShopServer.Util;

namespace OldManinTheShopServer.Util
{
    class UserVerificationUtil
    {
        public static bool LoginTokenValid(OverallUser databaseUser, string loginToken)
        {
            byte[] convertedText = Encoding.UTF8.GetBytes(databaseUser.LoggedTokens);
            DataContractJsonSerializer loggedTokenSerializer = new DataContractJsonSerializer(typeof(LoggedTokens));
            LoggedTokens dbTokens = loggedTokenSerializer.ReadObject(new MemoryStream(convertedText)) as LoggedTokens;
            if (dbTokens == null)
                throw new ArgumentException("database user had an invalid entry for logged tokens");
            if (!loginToken.Equals(dbTokens.BaseLoggedInToken))
                return false;
            DateTime dbExpiration = DateTime.Parse(dbTokens.BaseLoggedInTokenExpiration);
            return DateTime.UtcNow.CompareTo(dbExpiration) < 0;
        }

        public static bool AuthTokenValid(OverallUser databaseUser, string authToken)
        {
            byte[] convertedText = Encoding.UTF8.GetBytes(databaseUser.LoggedTokens);
            DataContractJsonSerializer loggedTokenSerializer = new DataContractJsonSerializer(typeof(LoggedTokens));
            LoggedTokens dbTokens = loggedTokenSerializer.ReadObject(new MemoryStream(convertedText)) as LoggedTokens;
            if (dbTokens == null)
                throw new ArgumentException("database user had an invalid entry for logged tokens");
            if (!authToken.Equals(dbTokens.AuthLoggedInToken))
                return false;
            DateTime dbExpiration = DateTime.Parse(dbTokens.AuthLoggedInTokenExpiration);
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
                    byte[] pass = decrypt.TransformFinalBlock(databaseUser.AuthToken, 0, databaseUser.AuthToken.Length);
                    toTest = Encoding.UTF8.GetString(pass);
                }
                catch (CryptographicException)
                {
                    return false;
                }
            }
            return toTest.Equals("pass");
        }
    }
}

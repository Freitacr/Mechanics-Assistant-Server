using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using MechanicsAssistantServer.Data.MySql.TableDataTypes;
using System.Runtime.Serialization;

namespace MechanicsAssistantServer.Util
{
    class UserVerificationUtil
    {
        public static bool LoginTokenValid(OverallUser databaseUser, string loggedTokenJSON)
        {
            byte[] convertedText = Encoding.UTF32.GetBytes(databaseUser.LoggedTokens);
            DataContractJsonSerializer loggedTokenSerializer = new DataContractJsonSerializer(typeof(LoggedTokens));
            LoggedTokens dbTokens = loggedTokenSerializer.ReadObject(new MemoryStream(convertedText)) as LoggedTokens;
            if (dbTokens == null)
                throw new ArgumentException("database user had an invalid entry for logged tokens");
            convertedText = Encoding.UTF32.GetBytes(loggedTokenJSON);
            LoggedTokens reqTokens;
            try
            {
                reqTokens = loggedTokenSerializer.ReadObject(new MemoryStream(convertedText)) as LoggedTokens;
            } catch (SerializationException)
            {
                return false;
            }
            if (reqTokens == null)
                return false;
            if (!reqTokens.BaseLoggedInToken.Equals(dbTokens.BaseLoggedInToken))
                return false;
            DateTime dbExpiration = DateTime.Parse(dbTokens.BaseLoggedInTokenExpiration);
            DateTime reqExpiration = DateTime.Parse(reqTokens.BaseLoggedInTokenExpiration);
            return reqExpiration.CompareTo(dbExpiration) < 0;
        }

    }
}

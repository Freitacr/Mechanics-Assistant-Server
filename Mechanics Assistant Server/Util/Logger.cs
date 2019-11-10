using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OldManInTheShopServer.Util
{
    

    public class Logger : IDisposable 
    {
        private static Logger Instance = null;
        public static string FileLocation = null;

        private readonly StreamWriter WriterOut;

        public enum LogLevel
        {
            WARNING,
            ERROR,
            INFO
        }

        public static Logger Global
        {
            get
            {
                if(Instance == null)
                {
                    if (FileLocation == null)
                        Instance = new Logger("log.txt");
                    else Instance = new Logger(FileLocation);
                }
                return Instance;
            }
        }

        public Logger(string fileLocation)
        {
            WriterOut = new StreamWriter(fileLocation);
        }

        public void Log(LogLevel logLevel, string message)
        {
            WriterOut.WriteLine(logLevel.ToString("{0}: " + message));
        }

        public void Dispose()
        {
            WriterOut.Dispose();
        }
    }


}

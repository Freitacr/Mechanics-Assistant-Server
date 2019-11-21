using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OldManInTheShopServer.Util
{ 
    /// <summary>
    /// Class designed to be a one-shot mass disposer for the active global loggers
    /// </summary>
    public class LoggerDisposer : IDisposable
    {
        private Dictionary<string, Logger> Closables;
        public LoggerDisposer(Dictionary<string, Logger> toClose)
        {
            Closables = toClose;
        }

        public void Dispose()
        {

            foreach (Logger l in Closables.Values)
            {
                try
                {
                    l.Dispose();
                } catch(ObjectDisposedException)
                {
                    //do nothing, all is fine.
                }
                catch (NullReferenceException)
                {
                    //do nothing, all is fine.
                }
            }
        }
    }

    public class Logger : IDisposable 
    {
        private static Dictionary<string, Logger> Loggers = new Dictionary<string, Logger>();
        public static LoggerDisposer Disposer = new LoggerDisposer(Loggers);

        private readonly StreamWriter WriterOut;

        public static class LoggerDefaultFileLocations
        {
            public static readonly string EXCEPTIONS = "errors.txt";
            public static readonly string DEFAULT = "log.txt";
            public static readonly string NETEXCEPTIONS = "netErrors.txt";
        }

        public enum LogLevel
        {
            WARNING,
            ERROR,
            INFO
        }

        public static Logger GetLogger(string fileLocation)
        {
            if (!Loggers.ContainsKey(fileLocation))
            {
                Loggers.Add(fileLocation, new Logger(fileLocation));
            }
            return Loggers[fileLocation];
        }

        public Logger(string fileLocation)
        {
            WriterOut = new StreamWriter(fileLocation);
        }

        public void Log(LogLevel logLevel, string message)
        {
            WriterOut.WriteLine(logLevel.ToString() + ": " + message);
            WriterOut.Flush();
        }

        public void Dispose()
        {
            WriterOut.Dispose();
        }
    }


}

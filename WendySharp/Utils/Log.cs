using System;
using System.IO;

namespace WendySharp
{
    static class Log
    {
        private const string LOG_DIRECTORY = "logs";

        private enum Category
        {
            DEBUG,
            INFO,
            WARN,
            ERROR
        }

        private static readonly object logLock = new object();

        static Log()
        {
            try
            {
                var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LOG_DIRECTORY);
                Directory.CreateDirectory(logsDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Unable to create logs directory: {0}", ex.Message);
            }
        }

        public static void WriteDebug(string component, string format, params object[] args)
        {
            WriteLine(Category.DEBUG, component, format, args);
        }

        public static void WriteInfo(string component, string format, params object[] args)
        {
            WriteLine(Category.INFO, component, format, args);
        }

        public static void WriteWarn(string component, string format, params object[] args)
        {
            WriteLine(Category.WARN, component, format, args);
        }

        public static void WriteError(string component, string format, params object[] args)
        {
            WriteLine(Category.ERROR, component, format, args);
        }

        private static void WriteLine(Category category, string component, string format, params object[] args)
        {
            var logLine = $"{DateTime.Now.ToString("s").Replace('T', ' ')} [{category}] {component}: {string.Format(format, args)}{Environment.NewLine}";

            lock (logLock)
            {
                if (category == Category.ERROR)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(logLine);
                    Console.ResetColor();
                }
                else if (category == Category.DEBUG)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(logLine);
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(logLine);
                }
            }

            try
            {
                lock (logLock)
                {
                    File.AppendAllText(GetLogFile(), logLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to log to file: {0}", ex.Message);
            }
        }

        private static string GetLogFile()
        {
            var logFile = string.Format("{0}.log", DateTime.Now.ToString("MMMM_dd_yyyy"));

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LOG_DIRECTORY, logFile);
        }
    }
}

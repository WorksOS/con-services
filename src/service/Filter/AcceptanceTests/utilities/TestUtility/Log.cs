using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TestUtility
{
    public static class Log
    {
        public enum ContentType
        {
            URI,
            KafkaSend,
            KafkaResponse,
            ApiSend,
            ApiResponse,
            DbQuery,
            DbResult,
            Error,
        }

        public static void Error(string message, ContentType contentType)
        {
            WriteEntry(message, "Error", Enum.GetName(typeof(ContentType), contentType));
        }

        public static void Error(Exception ex, ContentType contentType)
        {
            WriteEntry(ex.Message, "Error", Enum.GetName(typeof(ContentType), contentType));
        }

        public static void Warning(string message, ContentType contentType)
        {
            WriteEntry(message, "Warning", Enum.GetName(typeof(ContentType), contentType));
        }

        public static void Info(string message, ContentType contentType)
        {
            WriteEntry(message, "Info", Enum.GetName(typeof(ContentType), contentType));
        }

        private static void WriteEntry(string message, string logType, string contentType)
        {
          // TODO (Aaron) Refactor, this isn't how we log test activity any longer.
          return;

            message = message == null ? "" : Regex.Replace(message, @"\s+|\n|\r", " ");
            string contents = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logType}] [{contentType}] {message}";
            using (StreamWriter w = File.AppendText("/app/testresults/accepttest.log")) //accepttest.log")) //"
            {
                w.WriteLine(contents);
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSP.MasterData.Project.AcceptanceTests.Utils
{
    /// <summary>
    /// A convenience class to format log message
    /// </summary>
    public static class LogFormatter
    {
        public enum ContentType
        {
            CreateProjectEvent,
            //TODO
            KafkaProduceResponse,

            URI,
            HttpMethod,
            RequestHeader,
            RequestBody,
            HttpCode,
            ResponseHeader,
            ResponseBody,
            Error,
            Fail
        }

        public static string Format(string msg, ContentType type)
        {
            return string.Format("[{0,-20}] {1}", Enum.GetName(typeof(ContentType), type), msg);
        }
    }
}

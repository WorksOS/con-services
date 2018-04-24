using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaptorSvcAcceptTestsCommon.Utils
{
    /// <summary>
    /// A convenience class to format log message
    /// </summary>
    public static class LogFormatter
    {
        public enum ContentType
        {
            URI,
            HttpMethod,
            Request,
            HttpCode,
            Response,
            Error,
            RequestHeader,
            ResponseHeader
        }

        public static string Format(string msg, ContentType type)
        {
            return string.Format("[{0,-20}] {1}", Enum.GetName(typeof(ContentType), type), msg);
        }
    }
}

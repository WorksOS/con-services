using System;

namespace Utilities.Logging.Models
{
    public class LogRequestContext
    {
        public LogRequestContext()
        {
            CorrelationId = Guid.NewGuid();
        }
        public Guid CorrelationId { get; set; }
        public string TraceId { get; set; }
        public string ServerName { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationVersion { get; set; }
        public int ProcessId { get; set; }
    }
}
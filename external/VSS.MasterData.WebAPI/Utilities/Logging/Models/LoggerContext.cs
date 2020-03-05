using System;
using System.Diagnostics;

namespace Utilities.Logging.Models
{
    public class LoggerContext : LogRequestContext
    {
        public string ClassMethod { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string AssemblyDetails { get; set; }

        public LoggerContext(string applicationName, string applicationVersion)
        {
            this.ServerName = Environment.MachineName;
            this.ProcessId = Process.GetCurrentProcess().Id;
            this.ApplicationName = applicationName;
            this.ApplicationVersion = applicationVersion;
        }
    }
}

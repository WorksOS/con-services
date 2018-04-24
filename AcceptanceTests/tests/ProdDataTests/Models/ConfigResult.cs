using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    /// <summary>
    /// The GET response body: Configuration of Raptor
    /// This is copied from ...\ReportSvc\ResultHandling\ConfigResult.cs
    /// </summary>
    public class ConfigResult : RequestResult, IEquatable<ConfigResult>
    {
        #region Members
        public string Configuration { get; set; } 
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public ConfigResult()
            : base("success")
        { } 
        #endregion

        #region Equality test
        public bool Equals(ConfigResult other)
        {
            if (other == null)
                return false;

            return this.Configuration == other.Configuration &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(ConfigResult a, ConfigResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(ConfigResult a, ConfigResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is ConfigResult && this == (ConfigResult)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region ToString override
        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>A string representation.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        #endregion
    }
}

using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    public class SummarySpeedRequest
    {
        /// <summary>
        /// The project to perform the request against
        /// </summary>
        public long projectID { get; set; }

        /// <summary>
        /// An identifying string from the caller
        /// </summary>
        public Guid? callId { get; set; }

        /// <summary>
        /// The filter to be used 
        /// </summary>
        public FilterResult filter { get; set; }

        /// <summary>
        /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
        /// </summary>
        public LiftBuildSettings liftBuildSettings { get; set; }
    } 
    #endregion

    #region Result
    public class SummarySpeedResult : RequestResult, IEquatable<SummarySpeedResult>
    {
        #region Members
        /// <summary>
        /// Cut volume in m3
        /// </summary>
        public double AboveTarget { get; set; }
        /// <summary>
        /// Fill volume in m3
        /// </summary>
        public double BelowTarget { get; set; }
        /// <summary>
        /// Cut area in m2
        /// </summary>
        public double MatchTarget { get; set; }
        /// <summary>
        /// Total coverage area (cut + fill + no change) in m2. No Coverage occurs where one of the design or production data pair being compared has no elevation. Where both of the pair have no elevation, nothing will be returned.
        /// </summary>
        public double CoverageArea { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public SummarySpeedResult()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(SummarySpeedResult other)
        {
            if (other == null)
                return false;

            return Math.Round(this.AboveTarget, 2) == Math.Round(other.AboveTarget, 2) &&
                Math.Round(this.BelowTarget, 2) == Math.Round(other.BelowTarget, 2) &&
                Math.Round(this.MatchTarget, 2) == Math.Round(other.MatchTarget, 2) &&
                Math.Round(this.CoverageArea, 2) == Math.Round(other.CoverageArea, 2) &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(SummarySpeedResult a, SummarySpeedResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(SummarySpeedResult a, SummarySpeedResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is SummarySpeedResult && this == (SummarySpeedResult)obj;
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
    #endregion
}

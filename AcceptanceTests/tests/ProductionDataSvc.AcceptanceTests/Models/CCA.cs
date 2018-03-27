using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// The request representation used for a summary CCA request.
    /// </summary>
    public class CCARequest
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
        /// The lift build settings to use in the request.
        /// </summary>
        public LiftBuildSettings liftBuildSettings { get; set; }

        /// <summary>
        /// The filter instance to use in the request
        /// Value may be null.
        /// </summary>
        public FilterResult filter { get; set; }

        /// <summary>
        /// The filter ID to used in the request.
        /// May be null.
        /// </summary>
        public long filterID { get; set; }
    } 
    #endregion

    /// <summary>
    /// The result representation of a summary CCA request
    /// </summary>
    public class CCASummaryResult : RequestResult, IEquatable<CCASummaryResult>
    {
        #region Members
        /// <summary>
        /// The percentage of cells that are complete within the target bounds
        /// </summary>
        public double completePercent { get; set; }

        /// <summary>
        /// The percentage of the cells that are over-complete
        /// </summary>
        public double overCompletePercent { get; set; }

        /// <summary>
        /// The internal result code of the request. Documented elsewhere.
        /// </summary>
        public short returnCode { get; set; }

        /// <summary>
        /// The total area covered by non-null cells in the request area
        /// </summary>
        public double totalAreaCoveredSqMeters { get; set; }

        /// <summary>
        /// The percentage of the cells that are under complete
        /// </summary>
        public double underCompletePercent { get; set; } 
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public CCASummaryResult()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(CCASummaryResult other)
        {
            if (other == null)
                return false;

            return Math.Round(this.completePercent, 2) == Math.Round(other.completePercent, 2) &&
                Math.Round(this.overCompletePercent, 2) == Math.Round(other.overCompletePercent, 2) &&
                this.returnCode == other.returnCode &&
                Math.Round(this.totalAreaCoveredSqMeters, 2) == Math.Round(other.totalAreaCoveredSqMeters, 2) &&
                Math.Round(this.underCompletePercent, 2) == Math.Round(other.underCompletePercent, 2) &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(CCASummaryResult a, CCASummaryResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(CCASummaryResult a, CCASummaryResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is CCASummaryResult && this == (CCASummaryResult)obj;
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

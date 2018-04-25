using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    /// <summary>
    /// Represents result returned by Summary Thickness request
    /// </summary>
    public class SummaryThicknessResult : RequestResult, IEquatable<SummaryThicknessResult>
    {
        #region Members
        /// <summary>
        /// Zone boundaries
        /// </summary>
        public BoundingBox3DGrid BoundingExtents { get; set; }

        /// <summary>
        /// AboveTarget in m2
        /// </summary>
        public double AboveTarget { get; set; }

        /// <summary>
        /// BelowTarget in m2
        /// </summary>
        public double BelowTarget { get; set; }

        /// <summary>
        /// MatchTarget in m2
        /// </summary>
        public double MatchTarget { get; set; }

        /// <summary>
        /// NoCoverageArea in m2
        /// </summary>
        public double NoCoverageArea { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public SummaryThicknessResult()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(SummaryThicknessResult other)
        {
            if (other == null) 
                return false;

            return Math.Round(this.BoundingExtents.maxX, 2) == Math.Round(other.BoundingExtents.maxX, 2) &&
                Math.Round(this.BoundingExtents.maxY, 2) == Math.Round(other.BoundingExtents.maxY, 2) &&
                Math.Round(this.BoundingExtents.maxZ, 2) == Math.Round(other.BoundingExtents.maxZ, 2) &&
                Math.Round(this.BoundingExtents.minX, 2) == Math.Round(other.BoundingExtents.minX, 2) &&
                Math.Round(this.BoundingExtents.minY, 2) == Math.Round(other.BoundingExtents.minY, 2) &&
                Math.Round(this.BoundingExtents.minZ, 2) == Math.Round(other.BoundingExtents.minZ, 2) &&
                Math.Round(this.AboveTarget, 2) == Math.Round(other.AboveTarget, 2) &&
                Math.Round(this.BelowTarget, 2) == Math.Round(other.BelowTarget, 2) &&
                Math.Round(this.MatchTarget, 2) == Math.Round(other.MatchTarget, 2) &&
                Math.Round(this.NoCoverageArea, 2) == Math.Round(other.NoCoverageArea, 2) &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(SummaryThicknessResult a, SummaryThicknessResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(SummaryThicknessResult a, SummaryThicknessResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is SummaryThicknessResult && this == (SummaryThicknessResult)obj;
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

using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// The representation of a elevation statistics request
    /// </summary>
    public class ElevationStatisticsRequest
    {
        /// <summary>
        /// The project to process the CS definition file into.
        /// </summary>
        /// 
        public long projectId { get; set; }

        /// <summary>
        /// An identifying string from the caller
        /// </summary>
        public Guid? callId { get; set; }

        /// <summary>
        /// The filter to be used for the request
        /// </summary>
        public FilterResult Filter { get; set; }

        /// <summary>
        /// The ID of the filter to be used for the request
        /// </summary>
        public long FilterID { get; set; }

        /// <summary>
        /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
        /// </summary>
        public LiftBuildSettings liftBuildSettings { get; set; }
    } 
    #endregion

    /// <summary>
    /// Represents result returned by levation Statistics request
    /// </summary>
    public class ElevationStatisticsResult : RequestResult, IEquatable<ElevationStatisticsResult>
    {
        #region Members
        /// <summary>
        /// Zone boundaries
        /// </summary>
        public BoundingBox3DGrid BoundingExtents { get; set; }
        /// <summary>
        /// Minimum elevation of cells tht matched the filter. 
        /// </summary>
        public double MinElevation { get; set; }
        /// <summary>
        /// Maximum elevation of cells tht matched the filter. 
        /// </summary>
        public double MaxElevation { get; set; }
        /// <summary>
        /// Total coverage area (cut + fill + no change) in m2. 
        /// </summary>
        public double TotalCoverageArea { get; set; } 
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public ElevationStatisticsResult()
            : base("success")
        { } 
        #endregion

        #region Equality test
        public bool Equals(ElevationStatisticsResult other)
        {
            if (other == null)
                return false;

          var bboxEqual = this.BoundingExtents != null && other.BoundingExtents != null ?
              Math.Round(this.BoundingExtents.maxX, 2) == Math.Round(other.BoundingExtents.maxX, 2) &&
              Math.Round(this.BoundingExtents.maxY, 2) == Math.Round(other.BoundingExtents.maxY, 2) &&
              Math.Round(this.BoundingExtents.maxZ, 2) == Math.Round(other.BoundingExtents.maxZ, 2) &&
              Math.Round(this.BoundingExtents.minX, 2) == Math.Round(other.BoundingExtents.minX, 2) &&
              Math.Round(this.BoundingExtents.minY, 2) == Math.Round(other.BoundingExtents.minY, 2) &&
              Math.Round(this.BoundingExtents.minZ, 2) == Math.Round(other.BoundingExtents.minZ, 2) :
              (this.BoundingExtents == null && other.BoundingExtents == null);

            return bboxEqual &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(ElevationStatisticsResult a, ElevationStatisticsResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(ElevationStatisticsResult a, ElevationStatisticsResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is ElevationStatisticsResult && this == (ElevationStatisticsResult)obj;
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

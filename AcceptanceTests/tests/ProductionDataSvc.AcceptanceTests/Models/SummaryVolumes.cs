using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// The representation of a summary volumes request.
    /// </summary>
    public class SummaryVolumesParameters : SummaryParametersBase
    {
        /// <summary>
        /// The type of volume computation to be performed as a summary volumes request
        /// </summary>
        public VolumesType volumeCalcType { get; set; }

        /// <summary>
        /// The descriptor of the design surface to be used as the base or earliest surface for design-filter volumes
        /// </summary>
        public DesignDescriptor baseDesignDescriptor { get; set; }

        /// <summary>
        /// The descriptor of the design surface to be used as the top or latest surface for filter-design volumes
        /// </summary>
        public DesignDescriptor topDesignDescriptor { get; set; }

        /// <summary>
        /// Sets the cut tolerance to calculate Summary Volumes.
        /// </summary>
        /// <value>
        /// The cut tolerance.
        /// </value>
        public double? CutTolerance { get; set; }

        /// <summary>
        /// Sets the fill tolerance to calculate Summary Volumes.
        /// </summary>
        /// <value>
        /// The cut tolerance.
        /// </value>
        public double? FillTolerance { get; set; } 
    }
    #endregion

    #region Result
    public class SummaryVolumes : RequestResult, IEquatable<SummaryVolumes>
    {
        #region Members
        /// <summary>
        /// Zone boundaries
        /// </summary>
        public BoundingBox3DGrid BoundingExtents { get; set; }
        /// <summary>
        /// Cut volume in m3
        /// </summary>
        public double Cut { get; set; }
        /// <summary>
        /// Fill volume in m3
        /// </summary>
        public double Fill { get; set; }
        /// <summary>
        /// Cut area in m2
        /// </summary>
        public double CutArea { get; set; }
        /// <summary>
        /// Fill area in m2
        /// </summary>
        public double FillArea { get; set; }
        /// <summary>
        /// Total coverage area (cut + fill + no change) in m2. 
        /// </summary>
        public double TotalCoverageArea { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public SummaryVolumes()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(SummaryVolumes other)
        {
            if (other == null) 
                return false;

            return Math.Round(this.BoundingExtents.maxX, 2) == Math.Round(other.BoundingExtents.maxX, 2) &&
                Math.Round(this.BoundingExtents.maxY, 2) == Math.Round(other.BoundingExtents.maxY, 2) &&
                Math.Round(this.BoundingExtents.maxZ, 2) == Math.Round(other.BoundingExtents.maxZ, 2) &&
                Math.Round(this.BoundingExtents.minX, 2) == Math.Round(other.BoundingExtents.minX, 2) &&
                Math.Round(this.BoundingExtents.minY, 2) == Math.Round(other.BoundingExtents.minY, 2) &&
                Math.Round(this.BoundingExtents.minZ, 2) == Math.Round(other.BoundingExtents.minZ, 2) &&
                Math.Round(this.Cut, 2) == Math.Round(other.Cut, 2) &&
                Math.Round(this.Fill, 2) == Math.Round(other.Fill, 2) &&
                Math.Round(this.CutArea, 2) == Math.Round(other.CutArea, 2) &&
                Math.Round(this.FillArea, 2) == Math.Round(other.FillArea, 2) &&
                Math.Round(this.TotalCoverageArea, 2) == Math.Round(other.TotalCoverageArea, 2) &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(SummaryVolumes a, SummaryVolumes b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(SummaryVolumes a, SummaryVolumes b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is SummaryVolumes && this == (SummaryVolumes)obj;
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

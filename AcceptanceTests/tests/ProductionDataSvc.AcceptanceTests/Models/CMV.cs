using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// The request representation used to request both detailed and summary CMV requests.
    /// </summary>
    public class CMVRequest
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
        /// The various summary and target values to use in preparation of the result
        /// </summary>
        public CMVSettings cmvSettings { get; set; }

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

        /// <summary>
        /// An override start date that applies to the operation in conjunction with any date range specified in a filter.
        /// Value may be null
        /// </summary>
        public DateTime? overrideStartUTC { get; set; }

        /// <summary>
        /// An override end date that applies to the operation in conjunction with any date range specified in a filter.
        /// Value may be null
        /// </summary>
        public DateTime? overrideEndUTC { get; set; }

        /// <summary>
        /// An override set of asset IDs that applies to the operation in conjunction with any asset IDs specified in a filter.
        /// Value may be null
        /// </summary>
        public List<long> overrideAssetIds { get; set; }
    }

    /// <summary>
    /// The parameters for CMV detailed and summary computations
    /// </summary>
    public class CMVSettings
    {
        /// <summary>
        /// The target CMV value expressed in 10ths of units
        /// </summary>
        public short cmvTarget { get; set; }

        /// <summary>
        /// The maximum CMV value to be considered 'compacted' expressed in 10ths of units
        /// </summary>
        public short maxCMV { get; set; }

        /// <summary>
        /// The maximum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
        /// </summary>
        public double maxCMVPercent { get; set; }

        /// <summary>
        /// The minimum CMV value to be considered 'compacted' expressed in 10ths of units
        /// </summary>
        public short minCMV { get; set; }

        /// <summary>
        /// The minimum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
        /// </summary>
        public double minCMVPercent { get; set; }

        /// <summary>
        /// Override the target CMV recorded from the machine with the value of cmvTarget
        /// </summary>
        public bool overrideTargetCMV { get; set; }
    }
    #endregion

    #region Result
    /// <summary>
    /// The result representation of a detailed CMV request
    /// </summary>
    public class CMVDetailedResult : RequestResult, IEquatable<CMVDetailedResult>
    {
        #region Members
        /// <summary>
        /// An array of percentages relating to the CMV values encountered in the processed cells.
        /// The percentages are for CMV values below the minimum, between the minimum and target, on target, between the target and the maximum and above the maximum CMV.
        /// </summary>
        public double[] percents { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public CMVDetailedResult()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(CMVDetailedResult other)
        {
            if (other == null)
                return false;

            if (this.percents.Length != other.percents.Length)
                return false;

            for (int i = 0; i < this.percents.Length; ++i)
                if (Math.Round(this.percents[i], 2) != Math.Round(other.percents[i], 2)) return false;

            return this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(CMVDetailedResult a, CMVDetailedResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(CMVDetailedResult a, CMVDetailedResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is CMVDetailedResult && this == (CMVDetailedResult)obj;
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

    /// <summary>
    /// The result representation of a summary CMV request
    /// </summary>
    public class CMVSummaryResult : RequestResult, IEquatable<CMVSummaryResult>
    {
        #region Members
        /// <summary>
        /// The percentage of cells that are compacted within the target bounds
        /// </summary>
        public double compactedPercent { get; set; }

        /// <summary>
        /// If the CMV value is constant, this is the constant value of all CMV targets in the processed data.
        /// </summary>
        public short constantTargetCMV { get; set; }

        /// <summary>
        /// Are the CMV target values applying to all processed cells constant?
        /// </summary>
        public bool isTargetCMVConstant { get; set; }

        /// <summary>
        /// The percentage of the cells that are over-compacted
        /// </summary>
        public double overCompactedPercent { get; set; }

        /// <summary>
        /// The internal result code of the request. Documented elsewhere.
        /// </summary>
        public short returnCode { get; set; }

        /// <summary>
        /// The total area covered by non-null cells in the request area
        /// </summary>
        public double totalAreaCoveredSqMeters { get; set; }

        /// <summary>
        /// The percentage of the cells that are under compacted
        /// </summary>
        public double underCompactedPercent { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public CMVSummaryResult()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(CMVSummaryResult other)
        {
            if (other == null)
                return false;

            return Math.Round(this.compactedPercent, 2) == Math.Round(other.compactedPercent, 2) &&
                this.constantTargetCMV == other.constantTargetCMV &&
                this.isTargetCMVConstant == other.isTargetCMVConstant &&
                Math.Round(this.overCompactedPercent, 2) == Math.Round(other.overCompactedPercent, 2) &&
                this.returnCode == other.returnCode &&
                Math.Round(this.totalAreaCoveredSqMeters, 2) == Math.Round(other.totalAreaCoveredSqMeters, 2) &&
                Math.Round(underCompactedPercent, 2) == Math.Round(other.underCompactedPercent, 2) &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(CMVSummaryResult a, CMVSummaryResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(CMVSummaryResult a, CMVSummaryResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is CMVSummaryResult && this == (CMVSummaryResult)obj;
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

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request

    /// <summary>
    /// The representation of a pass counts request
    /// </summary>
    public class PassCounts
    {
        /// <summary>
        /// The project to perform the request against. Required
        /// </summary>
        public long? projectID { get; set; }

        /// <summary>
        /// An identifier from the caller. 
        /// </summary>
        public Guid? callId { get; set; }

        /// <summary>
        /// Setting and configuration values related to processing pass count related queries
        /// </summary>
        public PassCountSettings passCountSettings { get; set; }

        /// <summary>
        /// A collection of parameters and configuration information relating to analysis and determination of material layers.
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
        /// </summary>
        public List<long> overrideAssetIds { get; set; }
    }

    /// <summary>
    /// The representation of a pass counts request for detailed pass counts
    /// </summary>
    public class DetailedPassCounts : PassCounts
    {
    }

    /// <summary>
    /// The representation of a pass counts request for summary pass counts
    /// </summary>
    public class SummaryPassCounts : PassCounts
    {
    }

    /// <summary>
    /// Setting and configuration values related to processing pass count related queries
    /// </summary>
    public class PassCountSettings
    {
        /// <summary>
        /// Is the request for a summary or detailed analysis of passcounts
        /// </summary>
        //public bool isSummary { get; set; }

        /// <summary>
        /// The array of passcount numbers to be accounted for in the pass count analysis. 
        /// This property is not used for a summary report only for a detailed report.
        /// There must be at least one item in the array and the first item's value should be > 0. 
        /// The values do not need to be evenly spaced but must increase.
        /// </summary>
        public int[] passCounts { get; set; }
    } 

    #endregion

    #region Result
    /// <summary>
    /// The represenation of the results of a detailed pass count request
    /// </summary>
    public class PassCountDetailedResult : RequestResult, IEquatable<PassCountDetailedResult>
    {
        #region Members
        /// <summary>
        /// Range of the target pass count values if all target pass counts relevant to analysed cell passes are the same.
        /// </summary>
        public TargetPassCountRange constantTargetPassCountRange { get; set; }

        /// <summary>
        /// Are all target pass counts relevant to analysed cell passes are the same?
        /// </summary>
        public bool isTargetPassCountConstant { get; set; }

        /// <summary>
        /// Collection of passcount percentages where each element represents the percentage of the matching index passcount number provided in the 
        /// passCounts member of the pass count request representation.
        /// </summary>
        public double[] percents { get; set; }

        /// <summary>
        /// Gets the total coverage area for the production data - not the total area specified in filter
        /// </summary>
        /// <value>
        /// The total coverage area in sq meters.
        /// </value>
        public double TotalCoverageArea { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public PassCountDetailedResult()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(PassCountDetailedResult other)
        {
            if (other == null)
                return false;

            if (this.percents.Length != other.percents.Length)
                return false;

            for (int i = 0; i < this.percents.Length; ++i)
                if (Math.Round(this.percents[i], 1) != Math.Round(other.percents[i], 1)) return false;

            return this.constantTargetPassCountRange.min == other.constantTargetPassCountRange.min &&
                this.constantTargetPassCountRange.max == other.constantTargetPassCountRange.max &&
                this.isTargetPassCountConstant == other.isTargetPassCountConstant &&
                Math.Round(this.TotalCoverageArea) == Math.Round(other.TotalCoverageArea) &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(PassCountDetailedResult a, PassCountDetailedResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(PassCountDetailedResult a, PassCountDetailedResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is PassCountDetailedResult && this == (PassCountDetailedResult)obj;
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
    /// The represenation of the results of a summary pass count request
    /// </summary>
    public class PassCountSummaryResult : RequestResult, IEquatable<PassCountSummaryResult>
    {
        #region Members
        /// <summary>
        /// Value of the target pass count if all target pass counts relevant to analysed cell passes are the same.
        /// </summary>
        public TargetPassCountRange constantTargetPassCountRange { get; set; }

        /// <summary>
        /// Are all target pass counts relevant to analysed cell passes are the same?
        /// </summary>
        public bool isTargetPassCountConstant { get; set; }

        /// <summary>
        /// The percentage of pass counts that match the target pass count specified in the passCountTarget member of the request
        /// </summary>
        public double percentEqualsTarget { get; set; }

        /// <summary>
        /// The percentage of pass counts that are greater than the target pass count specified in the passCountTarget member of the request
        /// </summary>
        public double percentGreaterThanTarget { get; set; }

        /// <summary>
        /// The percentage of pass counts that are less than the target pass count specified in the passCountTarget member of the request
        /// </summary>
        public double percentLessThanTarget { get; set; }

        /// <summary>
        /// The internal returnCode returned by the internal request. Documented elsewhere.
        /// </summary>
        public short returnCode { get; set; }

        /// <summary>
        /// The total area covered by non-null cells in the request area
        /// </summary>
        public double totalAreaCoveredSqMeters { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public PassCountSummaryResult()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(PassCountSummaryResult other)
        {
            if (other == null)
                return false;

            return this.constantTargetPassCountRange.min == other.constantTargetPassCountRange.min &&
                this.constantTargetPassCountRange.max == other.constantTargetPassCountRange.max &&
                this.isTargetPassCountConstant == other.isTargetPassCountConstant &&
                Math.Round(this.percentEqualsTarget, 1) == Math.Round(other.percentEqualsTarget, 1) &&
                Math.Round(this.percentGreaterThanTarget, 1) == Math.Round(other.percentGreaterThanTarget, 1) &&
                Math.Round(this.percentLessThanTarget, 1) == Math.Round(other.percentLessThanTarget, 1) &&
                this.returnCode == other.returnCode &&
                Math.Round(this.totalAreaCoveredSqMeters) == Math.Round(other.totalAreaCoveredSqMeters) &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(PassCountSummaryResult a, PassCountSummaryResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(PassCountSummaryResult a, PassCountSummaryResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is PassCountSummaryResult && this == (PassCountSummaryResult)obj;
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
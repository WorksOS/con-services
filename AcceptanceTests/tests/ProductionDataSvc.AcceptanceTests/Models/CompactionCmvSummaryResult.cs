using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionCmvSummaryResult : RequestResult, IEquatable<CompactionCmvSummaryResult>
  {
    /// <summary>
    /// The CMV summary data results
    /// </summary>
    public CmvSummaryData cmvSummaryData { get; set; }
    
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionCmvSummaryResult()
      : base("success")
    { }

    #region Equality test
    public bool Equals(CompactionCmvSummaryResult other)
    {
      if (other == null)
        return false;

      if (this.cmvSummaryData == null)
      {
        return this.Code == other.Code &&
               this.Message == other.Message;
      }

      return this.cmvSummaryData.Equals(other.cmvSummaryData) &&
       this.Code == other.Code &&
       this.Message == other.Message;
    }

    public static bool operator ==(CompactionCmvSummaryResult a, CompactionCmvSummaryResult b)
    {
      if ((object)a == null || (object)b == null)
        return Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionCmvSummaryResult a, CompactionCmvSummaryResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionCmvSummaryResult && this == (CompactionCmvSummaryResult)obj;
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

    /// <summary>
    /// CMV summary data returned
    /// </summary>
    public class CmvSummaryData : IEquatable<CmvSummaryData>
    {
      /// <summary>
      /// The percentage of cells that are compacted within the target bounds
      /// </summary>
      public double percentEqualsTarget { get; set; }
      /// <summary>
      /// The percentage of the cells that are over-compacted
      /// </summary>
      public double percentGreaterThanTarget { get; set; }
      /// <summary>
      /// The percentage of the cells that are under compacted
      /// </summary>
      public double percentLessThanTarget { get; set; }
      /// <summary>
      /// The total area covered by non-null cells in the request area
      /// </summary>
      public double totalAreaCoveredSqMeters { get; set; }
      /// <summary>
      /// CMV machine target and whether it is constant or varies.
      /// </summary>
      public CmvTargetData cmvTarget { get; set; }
      /// <summary>
      /// The minimum percentage the measured CMV may be compared to the cmvTarget from the machine
      /// </summary>
      public double minCMVPercent { get; set; }
      /// <summary>
      /// The maximum percentage the measured CMV may be compared to the cmvTarget from the machine
      /// </summary>
      public double maxCMVPercent { get; set; }

      public bool Equals(CmvSummaryData other)
      {
        if (other == null)
          return false;

        return Math.Round(this.percentEqualsTarget, 2) == Math.Round(other.percentEqualsTarget, 2) &&
               Math.Round(this.percentGreaterThanTarget, 2) == Math.Round(other.percentGreaterThanTarget, 2) &&
               Math.Round(this.percentLessThanTarget, 2) == Math.Round(other.percentLessThanTarget, 2) &&
               Math.Round(this.totalAreaCoveredSqMeters, 2) == Math.Round(other.totalAreaCoveredSqMeters, 2) &&
               this.cmvTarget.Equals(other.cmvTarget) &&
               Math.Round(this.minCMVPercent, 2) == Math.Round(other.minCMVPercent, 2) &&
               Math.Round(this.maxCMVPercent, 2) == Math.Round(other.maxCMVPercent, 2);
      }

    }

    /// <summary>
    /// CMV target data returned
    /// </summary>
    public class CmvTargetData : IEquatable<CmvTargetData>
    {
      /// <summary>
      /// If the CMV value is constant, this is the constant value of all CMV targets in the processed data.
      /// </summary>
      public double cmvMachineTarget { get; set; }
      /// <summary>
      /// Are the CMV target values applying to all processed cells varying?
      /// </summary>
      public bool targetVaries { get; set; }

      public bool Equals(CmvTargetData other)
      {
        if (other == null)
          return false;

        return this.cmvMachineTarget == other.cmvMachineTarget &&
               this.targetVaries == other.targetVaries;
      }
    }
  }
}

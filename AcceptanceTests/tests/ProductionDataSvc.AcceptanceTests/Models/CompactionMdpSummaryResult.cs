using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionMdpSummaryResult : RequestResult, IEquatable<CompactionMdpSummaryResult>
  {
    #region Members
    /// <summary>
    /// The MDP summary data results
    /// </summary>
    public MdpSummaryData mdpSummaryData { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionMdpSummaryResult()
            : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionMdpSummaryResult other)
    {
      if (other == null)
        return false;

      if (this.mdpSummaryData == null)
      {
        return this.Code == other.Code &&
               this.Message == other.Message;
      }

      return this.mdpSummaryData.Equals(other.mdpSummaryData) &&
       this.Code == other.Code &&
       this.Message == other.Message;
    }

    public static bool operator ==(CompactionMdpSummaryResult a, CompactionMdpSummaryResult b)
    {
      if ((object)a == null || (object)b == null)
        return object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionMdpSummaryResult a, CompactionMdpSummaryResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionMdpSummaryResult && this == (CompactionMdpSummaryResult)obj;
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
    /// MDP summary data returned
    /// </summary>
    public class MdpSummaryData : IEquatable<MdpSummaryData>
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
      /// MDP machine target and whether it is constant or varies.
      /// </summary>
      public MdpTargetData mdpTarget { get; set; }
      /// <summary>
      /// The minimum percentage the measured MDP may be compared to the mdpTarget from the machine
      /// </summary>
      public double minMDPPercent { get; set; }
      /// <summary>
      /// The maximum percentage the measured MDP may be compared to the mdpTarget from the machine
      /// </summary>
      public double maxMDPPercent { get; set; }

      public bool Equals(MdpSummaryData other)
      {
        if (other == null)
          return false;

        return Math.Round(this.percentEqualsTarget, 2) == Math.Round(other.percentEqualsTarget, 2) &&
               Math.Round(this.percentGreaterThanTarget, 2) == Math.Round(other.percentGreaterThanTarget, 2) &&
               Math.Round(this.percentLessThanTarget, 2) == Math.Round(other.percentLessThanTarget, 2) &&
               Math.Round(this.totalAreaCoveredSqMeters, 2) == Math.Round(other.totalAreaCoveredSqMeters, 2) &&
               this.mdpTarget.Equals(other.mdpTarget) &&
               Math.Round(this.minMDPPercent, 2) == Math.Round(other.minMDPPercent, 2) &&
               Math.Round(this.maxMDPPercent, 2) == Math.Round(other.maxMDPPercent, 2);
      }

    }

    /// <summary>
    /// MDP target data returned
    /// </summary>
    public class MdpTargetData : IEquatable<MdpTargetData>
    {
      /// <summary>
      /// If the MDP value is constant, this is the constant value of all MDP targets in the processed data.
      /// </summary>
      public double mdpMachineTarget { get; set; }
      /// <summary>
      /// Are the MDP target values applying to all processed cells varying?
      /// </summary>
      public bool targetVaries { get; set; }

      public bool Equals(MdpTargetData other)
      {
        if (other == null)
          return false;

        return this.mdpMachineTarget == other.mdpMachineTarget &&
               this.targetVaries == other.targetVaries;
      }
    }
  }
}

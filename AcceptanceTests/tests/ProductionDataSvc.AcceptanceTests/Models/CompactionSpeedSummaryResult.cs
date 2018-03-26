using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionSpeedSummaryResult : RequestResult, IEquatable<CompactionSpeedSummaryResult>
  {
    #region Members
    /// <summary>
    /// The Speed summary data results
    /// </summary>
    public SpeedSummaryData speedSummaryData { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionSpeedSummaryResult()
            : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionSpeedSummaryResult other)
    {
      if (other == null)
        return false;

      if (this.speedSummaryData == null)
      {
        return this.Code == other.Code &&
               this.Message == other.Message;
      }

      return this.speedSummaryData.Equals(other.speedSummaryData) &&
       this.Code == other.Code &&
       this.Message == other.Message;
    }

    public static bool operator ==(CompactionSpeedSummaryResult a, CompactionSpeedSummaryResult b)
    {
      if ((object)a == null || (object)b == null)
        return Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionSpeedSummaryResult a, CompactionSpeedSummaryResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionSpeedSummaryResult && this == (CompactionSpeedSummaryResult)obj;
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
    /// Speed summary data returned
    /// </summary>
    public class SpeedSummaryData : IEquatable<SpeedSummaryData>
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
      /// The minimum percentage the measured Speed may be compared to the speedTarget from the machine
      /// </summary>
      public double minTarget { get; set; }
      /// <summary>
      /// The maximum percentage the measured Speed may be compared to the speedTarget from the machine
      /// </summary>
      public double maxTarget { get; set; }

      public bool Equals(SpeedSummaryData other)
      {
        if (other == null)
          return false;

        return Math.Round(this.percentEqualsTarget, 2) == Math.Round(other.percentEqualsTarget, 2) &&
               Math.Round(this.percentGreaterThanTarget, 2) == Math.Round(other.percentGreaterThanTarget, 2) &&
               Math.Round(this.percentLessThanTarget, 2) == Math.Round(other.percentLessThanTarget, 2) &&
               Math.Round(this.totalAreaCoveredSqMeters, 2) == Math.Round(other.totalAreaCoveredSqMeters, 2) &&
               this.minTarget == other.minTarget &&
               this.maxTarget == other.maxTarget;
      }

    }

  }
}

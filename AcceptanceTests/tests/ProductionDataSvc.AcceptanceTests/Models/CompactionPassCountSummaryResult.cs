using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionPassCountSummaryResult : RequestResult, IEquatable<CompactionPassCountSummaryResult>
  {
    #region Members
    /// <summary>
    /// The PassCount summary data results
    /// </summary>
    public PassCountSummaryData passCountSummaryData { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionPassCountSummaryResult()
            : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionPassCountSummaryResult other)
    {
      if (other == null)
        return false;

      return this.passCountSummaryData.Equals(other.passCountSummaryData) &&
       this.Code == other.Code &&
       this.Message == other.Message;
    }

    public static bool operator ==(CompactionPassCountSummaryResult a, CompactionPassCountSummaryResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionPassCountSummaryResult a, CompactionPassCountSummaryResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionPassCountSummaryResult && this == (CompactionPassCountSummaryResult)obj;
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
    /// PassCount summary data returned
    /// </summary>
    public class PassCountSummaryData : IEquatable<PassCountSummaryData>
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
      /// The minimum value the measured PassCount may be compared to the passCountTarget from the machine
      /// </summary>
      public int minTarget { get; set; }
      /// <summary>
      /// The maximum value the measured PassCount may be compared to the passCountTarget from the machine
      /// </summary>
      public int maxTarget { get; set; }

      public bool Equals(PassCountSummaryData other)
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

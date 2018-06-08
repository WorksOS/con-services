using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionPassCountDetailedResult : RequestResult, IEquatable<CompactionPassCountDetailedResult>
  {
    #region Members
    /// <summary>
    /// The Pass Count details data results
    /// </summary>
    public PassCountDetailsData passCountDetailsData { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionPassCountDetailedResult()
            : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionPassCountDetailedResult other)
    {
      if (other == null)
        return false;

      if (this.passCountDetailsData == null)
      {
        return this.Code == other.Code &&
               this.Message == other.Message;
      }

      return this.passCountDetailsData.Equals(other.passCountDetailsData) &&
       this.Code == other.Code &&
       this.Message == other.Message;
    }

    public static bool operator ==(CompactionPassCountDetailedResult a, CompactionPassCountDetailedResult b)
    {
      if ((object)a == null || (object)b == null)
        return Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionPassCountDetailedResult a, CompactionPassCountDetailedResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionPassCountDetailedResult && this == (CompactionPassCountDetailedResult)obj;
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
    /// Pass Count details data returned.
    /// </summary>
    public class PassCountDetailsData
    {
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
      public double totalCoverageArea { get; set; }
      /// <summary>
      /// The minimum value the measured PassCount may be compared to the passCountTarget from the machine
      /// </summary>
      public PassCountTargetData PassCountTarget { get; set; }

      public bool Equals(PassCountDetailsData other)
      {
        if (other == null)
        {
          return false;
        }

        if (other.PassCountTarget != null && this.PassCountTarget != null)
        {
          return Common.ArraysOfDoublesAreEqual(this.percents, other.percents) &&
                 Math.Round(this.totalCoverageArea, 2) == Math.Round(other.totalCoverageArea, 2) &&
                 this.PassCountTarget.MaxPassCountMachineTarget == other.PassCountTarget.MaxPassCountMachineTarget &&
                 this.PassCountTarget.MinPassCountMachineTarget == other.PassCountTarget.MinPassCountMachineTarget;
        }

        return Common.ArraysOfDoublesAreEqual(this.percents, other.percents) &&
               Math.Round(this.totalCoverageArea, 2) == Math.Round(other.totalCoverageArea, 2);
      }
    }
  }
}
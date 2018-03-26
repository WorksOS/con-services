using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionCmvPercentChangeResult : RequestResult, IEquatable<CompactionCmvPercentChangeResult>
  {
    #region Members
    /// <summary>
    /// The CMV % change data results
    /// </summary>
    public CmvChangeSummaryData cmvChangeData { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionCmvPercentChangeResult()
            : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionCmvPercentChangeResult other)
    {
      if (other == null)
        return false;

      if (this.cmvChangeData == null)
      {
        return this.Code == other.Code &&
               this.Message == other.Message;
      }

      return this.cmvChangeData.Equals(other.cmvChangeData) &&
        this.Code == other.Code && this.Message == other.Message;
    }

    public static bool operator ==(CompactionCmvPercentChangeResult a, CompactionCmvPercentChangeResult b)
    {
      if ((object)a == null || (object)b == null)
        return Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionCmvPercentChangeResult a, CompactionCmvPercentChangeResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionCmvPercentChangeResult && this == (CompactionCmvPercentChangeResult)obj;
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
    /// CMV % change data returned
    /// </summary>
    public class CmvChangeSummaryData : IEquatable<CmvChangeSummaryData>
    {
      /// <summary>
      /// The CMV % change values
      /// </summary>
      public double[] percents { get; set; }
      /// <summary>
      /// The total area covered by non-null cells in the request area
      /// </summary>
      public double totalAreaCoveredSqMeters { get; set; }

      #region Equality test
      public static bool operator ==(CmvChangeSummaryData a, CmvChangeSummaryData b)
      {
        if ((object)a == null || (object)b == null)
          return Object.Equals(a, b);

        return a.Equals(b);
      }

      public static bool operator !=(CmvChangeSummaryData a, CmvChangeSummaryData b)
      {
        return !(a == b);
      }

      public bool Equals(CmvChangeSummaryData other)
      {
        if (other == null)
          return false;

        if (this.percents.Length != other.percents.Length)
          return false;

        for (int i = 0; i < this.percents.Length; ++i)
        {
          if (Math.Round(this.percents[i], 1) != Math.Round(other.percents[i], 1))
            return false;
        }

        return Math.Round(this.totalAreaCoveredSqMeters, 2) == Math.Round(other.totalAreaCoveredSqMeters, 2);
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
}

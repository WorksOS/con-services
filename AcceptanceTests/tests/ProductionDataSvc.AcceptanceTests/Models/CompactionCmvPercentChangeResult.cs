using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionCmvPercentChangeResult : RequestResult, IEquatable<CompactionCmvPercentChangeResult>
  {
    #region Members
    /// <summary>
    /// The CMV % change data results
    /// </summary>
    public CmvChangeSummaryData[] cmvChangeData { get; set; }
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

      if (this.cmvChangeData.Length != other.cmvChangeData.Length)
        return false;

      for (int i = 0; i < this.cmvChangeData.Length; ++i)
      {
        if (this.cmvChangeData[i] != other.cmvChangeData[i])
          return false;
      }
      return this.Code == other.Code && this.Message == other.Message;
    }

    public static bool operator ==(CompactionCmvPercentChangeResult a, CompactionCmvPercentChangeResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

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
      /// The range that the CMV % change value is for
      /// </summary>
      public double[] percentRange { get; set; }

      /// <summary>
      /// The CMV % change value
      /// </summary>
      public double percentValue { get; set; }

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

        if (this.percentRange.Length != other.percentRange.Length)
          return false;

        for (int i = 0; i < this.percentRange.Length; ++i)
        {
          if (Math.Round(this.percentRange[i], 1) != Math.Round(other.percentRange[i], 1))
            return false;
        }

        return Math.Round(this.percentValue, 2) == Math.Round(other.percentValue, 2);
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

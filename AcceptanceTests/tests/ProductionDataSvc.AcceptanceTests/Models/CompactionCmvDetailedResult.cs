using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionCmvDetailedResult : RequestResult, IEquatable<CompactionCmvDetailedResult>
  {
    #region Members
    /// <summary>
    /// An array of percentages relating to the CMV values encountered in the processed cells.
    /// The percentages are for CMV values between the minimum and target, on target, between the target and the maximum and above the maximum CMV.
    /// </summary>
    public double[] Percents { get; set; }
    /// <summary>
    /// CMV machine target and whether it is constant or varies.
    /// </summary>
    public CompactionCmvSummaryResult.CmvTargetData cmvTarget { get; set; }
    /// <summary>
    /// The minimum percentage the measured CMV may be compared to the cmvTarget from the machine
    /// </summary>
    public double minCMVPercent { get; set; }
    /// <summary>
    /// The maximum percentage the measured CMV may be compared to the cmvTarget from the machine
    /// </summary>
    public double maxCMVPercent { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionCmvDetailedResult() : base("success")
    {
      // ...
    }
    #endregion

    #region Equality test
    public bool Equals(CompactionCmvDetailedResult other)
    {
      if (other == null)
        return false;

      bool targetEqual = this.cmvTarget == null ? other.cmvTarget == null : this.cmvTarget.Equals(other.cmvTarget);

       return Common.ArraysOfDoublesAreEqual(this.Percents, other.Percents) &&
        targetEqual &&
        Math.Round(this.minCMVPercent, 2) == Math.Round(other.minCMVPercent, 2) &&
        Math.Round(this.maxCMVPercent, 2) == Math.Round(other.maxCMVPercent, 2) &&
        this.Code == other.Code &&
        this.Message == other.Message;
    }

    public static bool operator ==(CompactionCmvDetailedResult a, CompactionCmvDetailedResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionCmvDetailedResult a, CompactionCmvDetailedResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionCmvDetailedResult && this == (CompactionCmvDetailedResult)obj;
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

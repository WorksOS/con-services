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

       return Common.ArraysOfDoublesAreEqual(this.Percents, other.Percents) &&
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

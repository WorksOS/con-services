using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionCutFillDetailedResult : RequestResult, IEquatable<CompactionCutFillDetailedResult>
  {
    const int DECIMAL_PLACES = 2;
    #region Members
    /// <summary>
    /// The Cut Fill details data results
    /// </summary>
    public double[] percents { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionCutFillDetailedResult()
      : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionCutFillDetailedResult other)
    {
      if (other == null)
        return false;

      return Common.ArraysOfDoublesAreEqual(this.percents, other.percents, DECIMAL_PLACES) &&
             this.Code == other.Code &&
             this.Message == other.Message;
    }

    public static bool operator ==(CompactionCutFillDetailedResult a, CompactionCutFillDetailedResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionCutFillDetailedResult a, CompactionCutFillDetailedResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionCutFillDetailedResult && this == (CompactionCutFillDetailedResult)obj;
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

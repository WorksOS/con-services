using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionTemperatureDetailResult : RequestResult, IEquatable<CompactionTemperatureDetailResult>
  {
    const int DECIMAL_PLACES = 2;

    #region Members
    /// <summary>
    /// Collection of temperature percentages where each element represents the percentage of the matching index temperature target range provided in the 
    /// temperatutre list member of the temperature details request representation.
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionTemperatureDetailResult()
      : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionTemperatureDetailResult other)
    {
      if (other == null)
        return false;

      return Common.ArraysOfDoublesAreEqual(this.Percents, other.Percents, DECIMAL_PLACES) &&
             this.Code == other.Code &&
             this.Message == other.Message;
    }

    public static bool operator ==(CompactionTemperatureDetailResult a, CompactionTemperatureDetailResult b)
    {
      if ((object)a == null || (object)b == null)
        return Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionTemperatureDetailResult a, CompactionTemperatureDetailResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionTemperatureDetailResult && this == (CompactionTemperatureDetailResult)obj;
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

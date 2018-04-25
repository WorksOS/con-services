using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class AlignmentStationRangeResult : RequestResult, IEquatable<AlignmentStationRangeResult>
  {
    #region Members

    /// <summary>
    /// StartStation for the alignment file
    /// </summary>
    public double StartStation { get; set; }
    /// <summary>
    /// EndStation for the alignment file
    /// </summary>
    public double EndStation { get; set; }

    #endregion

    #region Constructor
    /// <summary>
    /// Constructor: success result by default
    /// </summary>
    public AlignmentStationRangeResult()
        : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(AlignmentStationRangeResult other)
    {
      if (other == null)
        return false;
      
      return StartStation==other.StartStation && EndStation==other.EndStation &&
          this.Code == other.Code &&
          this.Message == other.Message;
    }

    public static bool operator ==(AlignmentStationRangeResult a, AlignmentStationRangeResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(AlignmentStationRangeResult a, AlignmentStationRangeResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is AlignmentStationRangeResult && this == (AlignmentStationRangeResult)obj;
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

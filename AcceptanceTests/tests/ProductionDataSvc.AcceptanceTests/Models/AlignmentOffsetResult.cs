using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class AlignmentOffsetResult : RequestResult, IEquatable<AlignmentOffsetResult>
  {
    #region Members

    /// <summary>
    /// StartOffset for the alignment file
    /// </summary>
    public double StartOffset { get; set; }
    /// <summary>
    /// EndOffset for the alignment file
    /// </summary>
    public double EndOffset { get; set; }

    #endregion

    #region Constructor
    /// <summary>
    /// Constructor: success result by default
    /// </summary>
    public AlignmentOffsetResult()
        : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(AlignmentOffsetResult other)
    {
      if (other == null)
        return false;
      
      return StartOffset==other.StartOffset && EndOffset==other.EndOffset &&
          this.Code == other.Code &&
          this.Message == other.Message;
    }

    public static bool operator ==(AlignmentOffsetResult a, AlignmentOffsetResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(AlignmentOffsetResult a, AlignmentOffsetResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is AlignmentOffsetResult && this == (AlignmentOffsetResult)obj;
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

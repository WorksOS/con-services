using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  class CompactionElevationPaletteResult : RequestResult, IEquatable<CompactionElevationPaletteResult>
  {
    #region Members
    /// <summary>
    /// The palette for displaying elevation values.
    /// </summary>
    public DetailPalette palette { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionElevationPaletteResult()
      : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionElevationPaletteResult other)
    {
      if (other == null)
        return false;

      var paletteEqual = this.palette != null && other.palette != null
        ? this.palette.Equals(other.palette)
        : this.palette == null && other.palette == null;

      return paletteEqual &&
             this.Code == other.Code &&
             this.Message == other.Message;
    }

    public static bool operator ==(CompactionElevationPaletteResult a, CompactionElevationPaletteResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionElevationPaletteResult a, CompactionElevationPaletteResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionElevationPaletteResult && this == (CompactionElevationPaletteResult)obj;
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

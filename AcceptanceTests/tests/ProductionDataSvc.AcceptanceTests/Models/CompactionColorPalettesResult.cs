using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionColorPalettesResult : RequestResult, IEquatable<CompactionColorPalettesResult>
  {
    #region Members
    /// <summary>
    /// The palette for displaying CMV detail values.
    /// </summary>
    public DetailPalette cmvDetailPalette { get; set; }
    /// <summary>
    /// The palette for displaying pass count detail values.
    /// </summary>
    public DetailPalette passCountDetailPalette { get; set; }
    /// <summary>
    /// The palette for displaying pass count summary values.
    /// </summary>
    public SummaryPalette passCountSummaryPalette { get; set; }
    /// <summary>
    /// The palette for displaying cut/fill values.
    /// </summary>
    public DetailPalette cutFillPalette { get; set; }
    /// <summary>
    /// The palette for displaying temperature summary values.
    /// </summary>
    public SummaryPalette temperatureSummaryPalette { get; set; }
    /// <summary>
    /// The palette for displaying CMV summary values.
    /// </summary>
    public SummaryPalette cmvSummaryPalette { get; set; }
    /// <summary>
    /// The palette for displaying MDP summary values.
    /// </summary>
    public SummaryPalette mdpSummaryPalette { get; set; }
    /// <summary>
    /// The palette for displaying CMV % change values.
    /// </summary>
    public DetailPalette cmvPercentChangePalette { get; set; }
    /// <summary>
    /// The palette for displaying speed summary values.
    /// </summary>
    public SummaryPalette speedSummaryPalette { get; set; }
    /// <summary>
    /// The palette for displaying temperature details values.
    /// </summary>
    public DetailPalette temperatureDetailPalette { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionColorPalettesResult()
            : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionColorPalettesResult other)
    {
      if (other == null)
        return false;

      return this.cmvDetailPalette.Equals(other.cmvDetailPalette) &&
        this.passCountDetailPalette.Equals(other.passCountDetailPalette) &&
        this.passCountSummaryPalette.Equals(other.passCountSummaryPalette) &&
        this.cutFillPalette.Equals(other.cutFillPalette) &&
        this.temperatureSummaryPalette.Equals(other.temperatureSummaryPalette) &&
        this.cmvSummaryPalette.Equals(other.cmvSummaryPalette) &&
        this.mdpSummaryPalette.Equals(other.mdpSummaryPalette) &&
        this.cmvPercentChangePalette.Equals(other.cmvPercentChangePalette) &&
        this.speedSummaryPalette.Equals(other.speedSummaryPalette) &&
        this.temperatureDetailPalette.Equals(other.temperatureDetailPalette) &&
       this.Code == other.Code &&
       this.Message == other.Message;
    }

    public static bool operator ==(CompactionColorPalettesResult a, CompactionColorPalettesResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionColorPalettesResult a, CompactionColorPalettesResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionColorPalettesResult && this == (CompactionColorPalettesResult)obj;
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

  /// <summary>
  /// Representation of a palette for details data, both integral (e.g. pass count) and continuous (e.g elevation)
  /// </summary>
  public class DetailPalette 
  {
    #region Members
    /// <summary>
    /// The color/value pairs for the palette. There must be at least one item in the list.
    /// The values must be in ascending order. For integral values (e.g. pass count) the color is
    /// used for the exact value. For continuous values (e.g. elevation) the color is used for all
    /// values that fall with the range from the value upto but excluding the next value in the list.
    /// </summary>
    public List<ColorValue> colorValues { get; set; }

    /// <summary>
    /// The color for values above the last value. 
    /// </summary>
    public uint? aboveLastColor { get; set; }

    /// <summary>
    /// The color for values below the first value. 
    /// </summary>
    public uint? belowFirstColor { get; set; }
    #endregion


    #region Equality test
    public bool Equals(DetailPalette other)
    {
      if (other == null)
        return false;

      if (this.colorValues.Count != other.colorValues.Count)
        return false;

      for (int i = 0; i < this.colorValues.Count; i++)
      {
        if (!this.colorValues[i].Equals(other.colorValues[i]))
          return false;
      }
      return this.aboveLastColor == other.aboveLastColor &&
        this.belowFirstColor == other.belowFirstColor;
    }

    public static bool operator ==(DetailPalette a, DetailPalette b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(DetailPalette a, DetailPalette b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is DetailPalette && this == (DetailPalette)obj;
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

  /// <summary>
  /// Representation of a palette for summary data
  /// </summary>
  public class SummaryPalette 
  {
    #region Members
    /// <summary>
    /// The color for values above the target. 
    /// </summary>
    public uint aboveTargetColor { get; set; }

    /// <summary>
    /// The color for values equal to a target or within a target range. 
    /// </summary>
    public uint onTargetColor { get; set; }

    /// <summary>
    /// The color for values below the target. 
    /// </summary>
    public uint belowTargetColor { get; set; }
    #endregion


    #region Equality test
    public bool Equals(SummaryPalette other)
    {
      if (other == null)
        return false;

      return this.aboveTargetColor == other.aboveTargetColor &&
        this.onTargetColor == other.onTargetColor &&
        this.belowTargetColor == other.belowTargetColor;
    }

    public static bool operator ==(SummaryPalette a, SummaryPalette b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(SummaryPalette a, SummaryPalette b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is SummaryPalette && this == (SummaryPalette)obj;
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

  /// <summary>
  /// Represents a color and value pair for a palette
  /// </summary>
  public class ColorValue 
  {
    #region Members
    /// <summary>
    /// The color for the palette. 
    /// </summary>
    public uint color { get; set; }

    /// <summary>
    /// The discrete value or start of a range the color represents. 
    /// </summary>
    public double value { get; set; }
    #endregion

    #region Equality test
    public bool Equals(ColorValue other)
    {
      if (other == null)
        return false;

      return this.color == other.color &&
        Math.Round(this.value, 1) == Math.Round(other.value, 1);
    }

    public static bool operator ==(ColorValue a, ColorValue b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(ColorValue a, ColorValue b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is ColorValue && this == (ColorValue)obj;
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

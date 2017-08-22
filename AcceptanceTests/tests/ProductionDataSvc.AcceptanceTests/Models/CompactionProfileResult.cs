using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionProfileResult : RequestResult, IEquatable<CompactionProfileResult>
  {
    #region members
    /// <summary>
    /// The grid distance between the two profile end points. For straight line profiles this is the geomtric plane distance between the points. 
    /// For alignment profiles this is the station distance between start and end locations on the alignment the profile is computed between.
    /// </summary>
    public double gridDistanceBetweenProfilePoints;

    /// <summary>
    /// The collection of cells produced by the query. Cells are ordered by increasing station value along the line or alignment.
    /// </summary>
    public List<CompactionProfileCell> points;
    #endregion

    #region constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionProfileResult()
      : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionProfileResult other)
    {
      if (other == null)
        return false;

      if (this.points.Count != other.points.Count)
        return false;

      for (int i = 0; i < this.points.Count; i++)
      {
        if (!this.points[i].Equals(other.points[i]))
          return false;
      }

      return Math.Round(this.gridDistanceBetweenProfilePoints,2) == Math.Round(other.gridDistanceBetweenProfilePoints, 2) &&
             this.Code == other.Code &&
             this.Message == other.Message;
    }

    public static bool operator ==(CompactionProfileResult a, CompactionProfileResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionProfileResult a, CompactionProfileResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionProfileResult && this == (CompactionProfileResult)obj;
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

  public class CompactionProfileCell : IEquatable<CompactionProfileCell>
  {

    #region members
    /// <summary>
    /// The type of cell, either a cell edge intersection or the mid point of a segment cutting the cell. A edge can also be the start of a gap in the data.
    /// </summary>
    public ProfileCellType cellType;

    /// <summary>
    /// The station value, or distance from start of the profile line at which the profile line intersects this cell for cell edges 
    /// or the mid point of the line segment cutting through the cell for mid point type cells.
    /// </summary>
    public double station;

    /// <summary>
    /// Elevation of first cell pass in the profile cell.
    /// </summary>
    public float firstPassHeight;

    /// <summary>
    /// Highest elevation recoreded for all cell passes involved in computation of this profile cell.
    /// </summary>
    public float highestPassHeight;

    /// <summary>
    /// The elevation of the last (in time) cell pass involved in computation of this profile cell.
    /// </summary>
    public float lastPassHeight;

    /// <summary>
    /// Lowest elevation recoreded for all cell passes involved in computation of this profile cell.
    /// </summary>
    public float lowestPassHeight;

    /// <summary>
    /// Last (in time) composite elevation recorded in the cell
    /// </summary>
    /// 
    public float lastCompositeHeight;

    /// <summary>
    /// Elevation of the design at the location of the center point of the cell.
    /// </summary>
    /// 
    public float designHeight;

    /// <summary>
    /// Raw CMV value.
    /// </summary>
    public float cmv;

    /// <summary>
    ///  CMV value expressed as a percentage of the Target CMV applicable at the time the cell pass that contributed the CMV value was recorded.
    /// </summary>
    public float cmvPercent;

    /// <summary>
    /// Elevation of the cell pass that contributed the CMV value.
    /// </summary>
    public float cmvHeight;

    /// <summary>
    /// MDP value expressed as a percentage of the Target MDP applicable at the time the cell pass that contributed the MDP value was recorded.
    /// </summary>
    public float mdpPercent;

    /// <summary>
    /// Elevation of the cell pass that contributed the MDP value.
    /// </summary>
    public float mdpHeight;

    /// <summary>
    /// Temperature value. Value expressed in Celcius.
    /// </summary>
    public float temperature;

    /// <summary>
    /// Elevation of the cell pass that contributed the temperature value.
    /// </summary>
    public float temperatureHeight;

    /// <summary>
    /// Number of passes contained in the top most layer analysed from the cell passes
    /// </summary>
    public int topLayerPassCount;

    /// <summary>
    /// The CCV percent change calculates change of the CCV in % between current and previous CCV % over target.
    /// </summary>
    public float cmvPercentChange;

    /// <summary>
    /// Speed value in cm/s
    /// </summary>
    public float speed;

    /// <summary>
    /// Elevation of the cell pass that contributed the speed value.
    /// </summary>
    public float speedHeight;

    /// <summary>
    /// Cut-fill value in meters. Cut values are positive, fill values are negative, zero is on grade.
    /// </summary>
    public float cutFill;

    /// <summary>
    /// Elevation of the cell pass that contributed the cut-fill value.
    /// </summary>
    public float cutFillHeight;

    /// <summary>
    /// The value in the pass count summary color palette to use for this cell. 
    /// </summary>
    public ValueTargetType passCountIndex;

    /// <summary>
    /// The value in the temperature summary color palette to use for this cell.
    /// </summary>
    public ValueTargetType temperatureIndex;

    /// <summary>
    /// The value in the CMV summary color palette to use for this cell. 
    /// </summary>
    public ValueTargetType cmvIndex;

    /// <summary>
    /// The value in the MDP summary color palette to use for this cell. 
    /// </summary>
    public ValueTargetType mdpIndex;

    /// <summary>
    /// The value in the speed summary color palette to use for this cell. 
    /// </summary>
    public ValueTargetType speedIndex;
    #endregion

    #region Equality test

    private bool FloatEquals(float f1, float f2)
    {
      if (float.IsNaN(f1) || float.IsNaN(f2))
        return float.IsNaN(f1) && float.IsNaN(f2);
 
      return Math.Round(f1, 2) == Math.Round(f2, 2);
    }

    public bool Equals(CompactionProfileCell other)
    {
      if (other == null)
        return false;

      return this.cellType == other.cellType &&
             Math.Round(this.station, 2) == Math.Round(other.station, 2) &&
             FloatEquals(this.firstPassHeight, other.firstPassHeight) &&
             FloatEquals(this.highestPassHeight, other.highestPassHeight) &&
             FloatEquals(this.lastPassHeight, other.lastPassHeight) &&
             FloatEquals(this.lowestPassHeight, other.lowestPassHeight) &&
             FloatEquals(this.lastCompositeHeight, other.lastCompositeHeight) &&
             FloatEquals(this.designHeight, other.designHeight) &&
             FloatEquals(this.cmv, other.cmv) &&
             FloatEquals(this.cmvPercent, other.cmvPercent) &&
             FloatEquals(this.cmvHeight, other.cmvHeight) &&
             FloatEquals(this.mdpPercent, other.mdpPercent) &&
             FloatEquals(this.mdpHeight, other.mdpHeight) &&
             FloatEquals(this.temperature, other.temperature) &&
             FloatEquals(this.temperatureHeight, other.temperatureHeight) &&
             this.topLayerPassCount == other.topLayerPassCount &&
             FloatEquals(this.cmvPercentChange, other.cmvPercentChange) &&
             FloatEquals(this.speed, other.speed) &&
             FloatEquals(this.speedHeight, other.speedHeight) &&
             FloatEquals(this.cutFill, other.cutFill) &&
             FloatEquals(this.cutFillHeight, other.cutFillHeight) &&
             this.passCountIndex == other.passCountIndex &&
             this.temperatureIndex == other.temperatureIndex &&
             this.cmvIndex == other.cmvIndex &&
             this.mdpIndex == other.mdpIndex &&
             this.speedIndex == other.speedIndex;
    }

    public static bool operator ==(CompactionProfileCell a, CompactionProfileCell b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionProfileCell a, CompactionProfileCell b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionProfileCell && this == (CompactionProfileCell)obj;
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
    /// Specifies the type of profile cell 
    /// </summary>
    public enum ProfileCellType
    {
      /// <summary>
      /// Station intersects the cell edge and has data
      /// </summary>
      Edge,

      /// <summary>
      /// Station is the midpoint of the line segment that cuts through the cell
      /// </summary>
      MidPoint,

      /// <summary>
      /// Station intersects the cell edge and has no data; the start of a gap
      /// </summary>
      Gap,
    }

    /// <summary>
    /// Specifies what the summary value represents in terms of the target
    /// </summary>
    public enum ValueTargetType
    {
      /// <summary>
      /// No value for this type of data for this cell
      /// </summary>
      NoData = -1,

      /// <summary>
      /// Value is above target
      /// </summary>
      AboveTarget = 0,

      /// <summary>
      /// Value is on target
      /// </summary>
      OnTarget = 1,

      /// <summary>
      /// Value is below target
      /// </summary>
      BelowTarget = 2
    }
  }
}

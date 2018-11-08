namespace VSS.Productivity3D.Common.ResultHandling
{
  /// <summary>
  /// The data for a compaction profile. The cellType flag determines the type of cell. The cell edges or intersections are used for changing color.
  /// The midpoints are used for plotting or graphing the results as a profile line. 
  /// </summary>
  public class CompactionProfileCell
  {
    /// <summary>
    /// The type of cell, either a cell edge intersection or the mid point of a segment cutting the cell. A edge can also be the start of a gap in the data.
    /// </summary>
    public ProfileCellType cellType;

    /// <summary>
    /// The station value, or distance from start of the profile line at which the profile line intersects this cell for cell edges 
    /// or the mid point of the line segment cutting through the cell for mid point type points.
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
    /// Minimum speed value in km/h
    /// </summary>
    public float minSpeed;

    /// <summary>
    /// Maximum speed value in km/h
    /// </summary>
    public float maxSpeed;

    /// <summary>
    /// Cut-fill value in meters. Cut values are positive, fill values are negative, zero is on grade.
    /// </summary>
    public float cutFill;

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

    /// <summary>
    /// Default constructor
    /// </summary>
    public CompactionProfileCell()
    { }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="cell">The cell to copy</param>
    public CompactionProfileCell(CompactionProfileCell cell)
    {
      cellType = cell.cellType;
      station = cell.station;
      firstPassHeight = cell.firstPassHeight;
      highestPassHeight = cell.highestPassHeight;
      lastPassHeight = cell.lastPassHeight;
      lowestPassHeight = cell.lowestPassHeight;
      lastCompositeHeight = cell.lastCompositeHeight;
      designHeight = cell.designHeight;
      cmv = cell.cmv;
      cmvPercent = cell.cmvPercent;
      cmvHeight = cell.cmvHeight;
      mdpPercent = cell.mdpPercent;
      mdpHeight = cell.mdpHeight;
      temperature = cell.temperature;
      temperatureHeight = cell.temperatureHeight;
      topLayerPassCount = cell.topLayerPassCount;
      cmvPercentChange = cell.cmvPercentChange;
      minSpeed = cell.minSpeed;
      maxSpeed = cell.maxSpeed;
      cutFill = cell.cutFill;
      passCountIndex = cell.passCountIndex;
      temperatureIndex = cell.temperatureIndex;
      cmvIndex = cell.cmvIndex;
      mdpIndex = cell.mdpIndex;
      speedIndex = cell.speedIndex;
    }
  }
}
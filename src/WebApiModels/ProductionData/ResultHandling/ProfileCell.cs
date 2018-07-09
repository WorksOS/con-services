using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// The collection of information for a single cell in the computed profile (which may contain many cells) as defined by the collection of cell passes that comprise it.
  /// Values are revelant to the cell as a whole. In the case of cell attributes, event or target information they represent the latest known values
  /// for those items as at the time of the last contributory cell pass in the cell that contained a known-value for the attribute in question.
  /// Composite elevations are elevations that are calculated from a combination of elevation information from production data sourced from TAG files produced
  /// by machine control systems, the elevation information obtained from dated topological surveys (surveyed surfaces).
  /// </summary>
  /// 
  public class ProfileCell
  {
    /// <summary>
    /// The station value, or distance from start of the profile line at which the profile line intersects this cell.
    /// </summary>
    /// 
    public double station;

    /// <summary>
    /// Elevation of first cell pass in the profile cell.
    /// </summary>
    /// 
    public float firstPassHeight;

    /// <summary>
    /// Highest elevation recoreded for all cell passes involved in computation of this profile cell.
    /// </summary>
    /// 
    public float highestPassHeight;

    /// <summary>
    /// The elevation of the last (in time) cell pass involved in computation of this profile cell.
    /// </summary>
    /// 
    public float lastPassHeight;

    /// <summary>
    /// Lowest elevation recoreded for all cell passes involved in computation of this profile cell.
    /// </summary>
    /// 
    public float lowestPassHeight;

    /// <summary>
    /// First (in time) composite elevation recorded in the cell
    /// </summary>
    /// 
    public float firstCompositeHeight;

    /// <summary>
    /// Highest composite elevation recorded in the cell
    /// </summary>
    /// 
    public float highestCompositeHeight;

    /// <summary>
    /// Last (in time) composite elevation recorded in the cell
    /// </summary>
    /// 
    public float lastCompositeHeight;

    /// <summary>
    /// Lowest composite elevation recorded in the cell
    /// </summary>
    /// 
    public float lowestCompositeHeight;

    /// <summary>
    /// Elevation of the design at the location of the center point of the cell.
    /// </summary>
    /// 
    public float designHeight;

    /// <summary>
    ///  CMV value expressed as a percentage of the Target CMV applicable at the time the cell pass that contributed the CMV value was recorded.
    /// </summary>
    /// 
    public float cmvPercent;

    /// <summary>
    /// Elevation of the cell pass that contributed the CMV value.
    /// </summary>
    /// 
    public float cmvHeight;


    /// <summary>
    /// The previous valid CMV value
    /// </summary>
    public float previousCmvPercent;

    /// <summary>
    /// MDP value expressed as a percentage of the Target CMV applicable at the time the cell pass that contributed the MDP value was recorded.
    /// </summary>
    /// 
    public float mdpPercent;

    /// <summary>
    /// Elevation of the cell pass that contributed the MDP value.
    /// </summary>
    /// 
    public float mdpHeight;

    /// <summary>
    /// Temperature value. Value expressed in Celcius.
    /// </summary>
    /// 
    public float temperature;

    /// <summary>
    /// Elevation of the cell pass that contributed the temperature value.
    /// </summary>
    /// 
    public float temperatureHeight;

    /// <summary>
    /// Unknown.
    /// </summary>
    /// 
    public int temperatureLevel;

    /// <summary>
    /// Number of passes contained in the top most layer analysed from the cell passes
    /// </summary>
    /// 
    public int topLayerPassCount;

    /// <summary>
    /// Pass count target application at the time the last cell pass that contributed to the top most layer was recorded
    /// </summary>
    /// 
    public TargetPassCountRange topLayerPassCountTargetRange;

    /// <summary>
    /// Unknown
    /// </summary>
    /// 
    public int passCountIndex;

    /// <summary>
    /// The thickness of the top most layer analysed from the cell passes. Value is expressed in meters.
    /// </summary>
    /// 
    public float topLayerThickness;

    /// <summary>
    /// Determine the minimum elevation of the measured production data elevation or composite elevation
    /// </summary>
    /// 
    public float minHeight => lowestPassHeight < lowestCompositeHeight ? lowestPassHeight : lowestCompositeHeight;

    /// <summary>
    /// Determine the maximum elevation of the measured production data elevation or composite elevation
    /// </summary>
    /// 
    public float maxHeight => highestPassHeight > highestCompositeHeight ? highestPassHeight : highestCompositeHeight;
    
    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation containing station and last pass elevation information from the profile cell</returns>
    /// 
    public override string ToString()
    {
      return $"Station:{station}, LastPassElevation:{lastPassHeight}";
    }
  }
}

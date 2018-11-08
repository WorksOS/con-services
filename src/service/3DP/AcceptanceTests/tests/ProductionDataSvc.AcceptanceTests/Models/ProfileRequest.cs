using System;
using System.Reflection;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// </summary>
  public class ProfileRequest : RequestBase
  {
    /// <summary>
    /// The project to perform the request against
    /// </summary>
    public long? projectID { get; set; }

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    public Guid? callId { get; set; }

    /// <summary>
    /// The type of profile to be generated.
    /// </summary>
    public ProductionDataType profileType { get; set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    public FilterResult filter { get; set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// Value may be null.
    /// </summary>
    public long? filterID { get; set; }

    /// <summary>
    /// The descriptor for an alignment centerline design to be used as the geometry along which the profile is generated
    /// Value may be null.
    /// </summary>
    public DesignDescriptor alignmentDesign { get; set; }

    /// <summary>
    /// A series of points along which to generate the profile. Coorinates are expressed in terms of the grid coordinate system used by the project. Values are expressed in meters.
    /// Value may be null.
    /// </summary>
    public ProfileGridPoints gridPoints { get; set; }

    /// <summary>
    /// A series of points along which to generate the profile. Coorinates are expressed in terms of the WGS84 lat/lon coordinates. Values are expressed in radians.
    /// Value may be null.
    /// </summary>
    public ProfileLLPoints wgs84Points { get; set; }

    /// <summary>
    /// The station on an alignment centerline design (if one is provided) to start computing the profile from. Values are expressed in meters.
    /// </summary>
    public double? startStation { get; set; }

    /// <summary>
    /// The station on an alignment centerline design (if one is provided) to finish computing the profile at. Values are expressed in meters.
    /// </summary>
    public double? endStation { get; set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    public LiftBuildSettings liftBuildSettings { get; set; }

    /// <summary>
    /// Return all analysed layers and cell passes along with the summary cell based results of the profile query
    /// </summary>
    public bool returnAllPassesAndLayers { get; set; }
  }

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
    #region Members
    /// <summary>
    /// The station value, or distance from start of the profile line at which the profile line intersects this cell.
    /// </summary>
    /// 
    public double station { get; set; }

    /// <summary>
    /// Elevation of first cell pass in the profile cell.
    /// </summary>
    /// 
    public float firstPassHeight { get; set; }

    /// <summary>
    /// Highest elevation recoreded for all cell passes involved in computation of this profile cell.
    /// </summary>
    /// 
    public float highestPassHeight { get; set; }

    /// <summary>
    /// The elevation of the last (in time) cell pass involved in computation of this profile cell.
    /// </summary>
    /// 
    public float lastPassHeight { get; set; }

    /// <summary>
    /// Lowest elevation recoreded for all cell passes involved in computation of this profile cell.
    /// </summary>
    /// 
    public float lowestPassHeight { get; set; }

    /// <summary>
    /// First (in time) composite elevation recorded in the cell
    /// </summary>
    /// 
    public float firstCompositeHeight { get; set; }

    /// <summary>
    /// Highest composite elevation recorded in the cell
    /// </summary>
    /// 
    public float highestCompositeHeight { get; set; }

    /// <summary>
    /// Last (in time) composite elevation recorded in the cell
    /// </summary>
    /// 
    public float lastCompositeHeight { get; set; }

    /// <summary>
    /// Lowest composite elevation recorded in the cell
    /// </summary>
    /// 
    public float lowestCompositeHeight { get; set; }

    /// <summary>
    /// Elevation of the design at the location of the center point of the cell.
    /// </summary>
    /// 
    public float designHeight { get; set; }

    /// <summary>
    ///  CMV value expressed as a percentage of the Target CMV applicable at the time the cell pass that contributed the CMV value was recorded.
    /// </summary>
    /// 
    public float cmvPercent { get; set; }

    /// <summary>
    /// Elevation of the cell pass that contributed the CMV value.
    /// </summary>
    /// 
    public float cmvHeight { get; set; }

    /// <summary>
    /// The previous valid CMV value
    /// </summary>
    public float previousCmvPercent { get; set; }

    /// <summary>
    /// MDP value expressed as a percentage of the Target CMV applicable at the time the cell pass that contributed the MDP value was recorded.
    /// </summary>
    /// 
    public float mdpPercent { get; set; }

    /// <summary>
    /// Elevation of the cell pass that contributed the MDP value.
    /// </summary>
    /// 
    public float mdpHeight { get; set; }

    /// <summary>
    /// Temperature value. Value expressed in Celcius.
    /// </summary>
    /// 
    public float temperature { get; set; }

    /// <summary>
    /// Elevation of the cell pass that contributed the temperature value.
    /// </summary>
    /// 
    public float temperatureHeight { get; set; }

    /// <summary>
    /// Unknown.
    /// </summary>
    /// 
    public int temperatureLevel { get; set; }

    /// <summary>
    /// Number of passes contained in the top most layer analysed from the cell passes
    /// </summary>
    /// 
    public int topLayerPassCount { get; set; }

    /// <summary>
    /// Pass count target application at the time the last cell pass that contributed to the top most layer was recorded
    /// </summary>
    /// 
    public TargetPassCountRange topLayerPassCountTargetRange { get; set; }

    /// <summary>
    /// Unknown
    /// </summary>
    /// 
    public int passCountIndex { get; set; }

    /// <summary>
    /// The thickness of the top most layer analysed from the cell passes. Value is expressed in meters.
    /// </summary>
    /// 
    public float topLayerThickness { get; set; }

    /// <summary>
    /// Determine the minimum elevation of the measured production data elevation or composite elevation
    /// </summary>
    /// 
    public float minHeight
    {
      get
      {
        return lowestPassHeight < lowestCompositeHeight ? lowestPassHeight : lowestCompositeHeight;
      }
    }

    /// <summary>
    /// Determine the maximum elevation of the measured production data elevation or composite elevation
    /// </summary>
    /// 
    public float maxHeight
    {
      get
      {
        return highestPassHeight > highestCompositeHeight ? highestPassHeight : highestCompositeHeight;
      }
    }
    #endregion

    #region Equality test
    public static bool operator ==(ProfileCell a, ProfileCell b)
    {
      PropertyInfo[] allProperties = typeof(ProfileCell).GetProperties();
      foreach (var property in allProperties)
      {
        if (property.PropertyType == typeof(TargetPassCountRange))
        {
          if ((TargetPassCountRange)property.GetValue(a) != (TargetPassCountRange)property.GetValue(b))
            return false;
        }
        else if (double.IsNaN(Convert.ToDouble(property.GetValue(a))) || double.IsNaN(Convert.ToDouble(property.GetValue(b))))
        {
          if (!(double.IsNaN(Convert.ToDouble(property.GetValue(a))) && double.IsNaN(Convert.ToDouble(property.GetValue(b)))))
            return false;
        }
        else
        {
          if (Math.Round(Convert.ToDouble(property.GetValue(a)), 2) != Math.Round(Convert.ToDouble(property.GetValue(b)), 2))
            return false;
        }
      }

      return true;
    }

    public static bool operator !=(ProfileCell a, ProfileCell b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is ProfileCell && this == (ProfileCell)obj;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
    #endregion
  }
}

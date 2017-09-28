namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// A point that combines WGS85 latitude and longitude with a station value. This is used as a part of the alignment based profile response 
  /// representation to define the geometry along which the profile computation as made.
  /// </summary>
  /// 
  public class StationLLPoint
  {
    /// <summary>
    /// Station of point. Value is expressed in meters.
    /// </summary>
    /// 
    public double station;

    /// <summary>
    /// Latitude of point. Value is expressed in radians.
    /// </summary>
    /// 
    public double lat;

    /// <summary>
    /// Latitude of point. Value is expressed in radians.
    /// </summary>
    /// 
    public double lng;
  }
}
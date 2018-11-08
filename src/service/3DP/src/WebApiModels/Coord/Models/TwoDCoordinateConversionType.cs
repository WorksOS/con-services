namespace VSS.Productivity3D.WebApiModels.Coord.Models
{
  /// <summary>
  /// The defined types of 2D coordinate conversions.
  /// </summary>
  /// 
  public enum TwoDCoordinateConversionType
  {
    /// <summary>
    /// 2D coordinate conversion from Latitude/Longitude to North/East.
    /// </summary>
    /// 
    LatLonToNorthEast = 0,

    /// <summary>
    /// 2D coordinate conversion from North/East to Latitude/Longitude.
    /// </summary>
    /// 
    NorthEastToLatLon = 1
  }
}
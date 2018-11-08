namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CoordinateConversionRequest : RequestBase
  {
    /// <summary>
    /// The project to request coordinate conversion for.
    /// </summary>
    public long projectId { get; set; }

    /// <summary>
    /// 2D coordinate conversion: 
    ///   0 - from Latitude/Longitude to North/East.
    ///   1 - from North/East to Latitude/Longitude.
    /// </summary>
    public TwoDCoordinateConversionType conversionType { get; set; }

    /// <summary>
    /// The list of coordinates for conversion.
    /// </summary>
    /// 
    public TwoDConversionCoordinate[] conversionCoordinates { get; set; }
  }

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

  /// <summary>
  /// A point specified in WGS84 Latitude/Longitude or North/East geographic Cartesian coordinates.
  /// </summary>
  public class TwoDConversionCoordinate
  {
    /// <summary>
    /// Either the Easting or WGS84 Longitude of the position expressed in meters or in radians respectively.
    /// </summary>
    public double x { get; set; }

    /// <summary>
    /// Either the Northing or WGS84 Latitude of the position expressed in meters or in radians respectively.
    /// </summary>
    public double y { get; set; }
  }

  public class CoordinateConversionResult : ResponseBase
  {
    public TwoDConversionCoordinate[] conversionCoordinates { get; set; }

    public CoordinateConversionResult()
        : base("success")
    { }
  }
}

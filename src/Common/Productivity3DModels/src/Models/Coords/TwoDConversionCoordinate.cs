using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models.Coords
{
  /// <summary>
  /// A point specified in WGS84 Latitude/Longitude or North/East geographic Cartesian coordinates.
  /// </summary>
  public class TwoDConversionCoordinate// : IValidatable
  {
    /// <summary>
    /// Either the Easting or WGS84 Longitude of the position expressed in meters or in radians respectively.
    /// </summary>
    [JsonProperty(PropertyName = "x", Required = Required.Always)]
    [Required]
    public double X { get; private set; }

    /// <summary>
    /// Either the Northing or WGS84 Latitude of the position expressed in meters or in radians respectively.
    /// </summary>
    [JsonProperty(PropertyName = "y", Required = Required.Always)]
    [Required]
    public double Y { get; private set; }

    
    /// <summary>
    /// Default private constructor.
    /// </summary>
    private TwoDConversionCoordinate()
    {}

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public TwoDConversionCoordinate(
        double x,
        double y
        )
    {
      X = x;
      Y = y;
    }

    /// <summary>
    /// Validates all properties.
    /// </summary>
    public void Validate()
    {
      //Nothing else to validate
    }
  }
}

using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Interfaces;


namespace VSS.Raptor.Service.WebApiModels.Coord.Models
{
  /// <summary>
  /// A point specified in WGS84 Latitude/Longitude or North/East geographic Cartesian coordinates.
  /// </summary>
  public class TwoDConversionCoordinate : IValidatable
  {
    /// <summary>
    /// Either the Easting or WGS84 Longitude of the position expressed in meters or in radians respectively.
    /// </summary>
    [JsonProperty(PropertyName = "x", Required = Required.Always)]
    public double x { get; private set; }

    /// <summary>
    /// Either the Northing or WGS84 Latitude of the position expressed in meters or in radians respectively.
    /// </summary>
    [JsonProperty(PropertyName = "y", Required = Required.Always)]
    public double y { get; private set; }

    
    /// <summary>
    /// Private constructor.
    /// </summary>
    private TwoDConversionCoordinate()
    {}

    /// <summary>
    /// Create an instance of the TwoDConversionCoordinate class.
    /// </summary>
    public static TwoDConversionCoordinate CreateTwoDConversionCoordinate(
        double x,
        double y
        )
    {
      return new TwoDConversionCoordinate
             {
                 x = x,
                 y = y
             };
    }

    /// <summary>
    /// Create a samble instance of the TwoDConversionCoordinate to display in Help documentation.
    /// </summary>
    public static TwoDConversionCoordinate HelpSample
    {
      get
      {
        return new TwoDConversionCoordinate()
        {
          x = 192.35,
          y = 234.12
        };
      }
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
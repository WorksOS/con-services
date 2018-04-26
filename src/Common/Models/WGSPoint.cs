using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Common.Interfaces;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// A point specified in WGS 84 latitude/longtitude coordinates
  /// </summary>
  [Obsolete("This should be aligned with the models package")]
  public class WGSPoint : IValidatable
  {
    private WGSPoint()
    { }

    /// <summary>
    /// WGS84 latitude, expressed in radians
    /// </summary>
    [DecimalIsWithinRange(-Math.PI / 2, Math.PI / 2)]
    [JsonProperty(PropertyName = "Lat", Required = Required.Always)]
    [Required]
    public double Lat { get; private set; }

    /// <summary>
    /// WSG84 longitude, expressed in radians
    /// </summary>
    [DecimalIsWithinRange(-Math.PI, Math.PI)]
    [JsonProperty(PropertyName = "Lon", Required = Required.Always)]
    [Required]
    public double Lon { get; private set; }

    /// <summary>
    /// Creates the point.
    /// </summary>
    /// <param name="lat">The latitude.</param>
    /// <param name="lon">The longtitude.</param>
    /// <returns></returns>
    public static WGSPoint CreatePoint(double lat, double lon)
    {
      return new WGSPoint { Lat = lat, Lon = lon };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      //nothign else to validate
    }
  }
}

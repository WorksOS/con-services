using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.MasterData.Models;
using System.Runtime.Serialization;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// A point specified in WGS 84 latitude/longtitude coordinates
  /// </summary>
  [Obsolete("This should be aligned with the models package")]
  [DataContract(Name = "WGSPoint")]
  public class WGSPoint3D 
  {
    /// <summary>
    /// Default private constructor.
    /// </summary>
    private WGSPoint3D()
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
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="lat">The latitude.</param>
    /// <param name="lon">The longtitude.</param>
    /// <returns></returns>
    public WGSPoint3D(double lat, double lon)
    {
      Lat = lat;
      Lon = lon;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      //nothing else to validate
    }
  }
}

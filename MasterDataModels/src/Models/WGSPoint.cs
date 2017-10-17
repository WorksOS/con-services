using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Interfaces;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// A point specified in WGS 84 latitude/longtitude coordinates
  /// </summary>
  public class WGSPoint : IValidatable, IEquatable<WGSPoint>
  {
    private WGSPoint()
    { }

    /// <summary>
    /// WGS84 latitude, expressed in degrees
    /// </summary>
    [DecimalIsWithinRange(-90, 90)]
    [JsonProperty(PropertyName = "Lat", Required = Required.Always)]
    [Required]
    public double Lat { get; private set; }

    /// <summary>
    /// WSG84 longitude, expressed in degrees
    /// </summary>
    [DecimalIsWithinRange(-180, 180)]
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
    public void Validate([FromServices] IServiceExceptionHandler serviceExceptionHandler)
    {
      //nothign else to validate
    }

    public bool Equals(WGSPoint other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Lat.Equals(other.Lat) && Lon.Equals(other.Lon);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((WGSPoint) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (Lat.GetHashCode() * 397) ^ Lon.GetHashCode();
      }
    }

    public static bool operator ==(WGSPoint left, WGSPoint right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(WGSPoint left, WGSPoint right)
    {
      return !Equals(left, right);
    }
  }
}
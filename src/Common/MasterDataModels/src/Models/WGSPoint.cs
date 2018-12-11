using System;
using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// A point specified in WGS 84 latitude/longtitude coordinates.
  /// </summary>
  public class WGSPoint : IEquatable<WGSPoint>
  {
    /// <summary>
    /// WGS84 latitude, expressed in degrees.
    /// </summary>
    [DecimalIsWithinRange(-90, 90)]
    [JsonProperty(PropertyName = "Lat", Required = Required.Always)]
    public double Lat { get; private set; }

    /// <summary>
    /// WSG84 longitude, expressed in degrees.
    /// </summary>
    [DecimalIsWithinRange(-180, 180)]
    [JsonProperty(PropertyName = "Lon", Required = Required.Always)]
    public double Lon { get; private set; }

    public WGSPoint(double latitude, double longtitude)
    {
      Lat = latitude;
      Lon = longtitude;
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
      if (obj.GetType() != GetType()) return false;

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

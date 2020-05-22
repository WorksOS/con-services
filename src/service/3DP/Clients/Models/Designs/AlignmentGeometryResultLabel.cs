using Newtonsoft.Json;

namespace VSS.Productivity3D.Productivity3D.Models.Designs
{
  public class AlignmentGeometryResultLabel
  {
    /// <summary>
    /// Measured (as in walked) distance along the alignment from the start of the alignment, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "stn", Required = Required.Always)]
    public double Stn { get; }

    /// <summary>
    /// Contains the WGS84 latitude (expressed decimal degrees) of the test insertion position
    /// </summary>
    [JsonProperty(PropertyName = "lat", Required = Required.Always)]
    public double Lat { get; }

    /// <summary>
    /// Contains the WGS84 longitude (expressed decimal degrees) of the test insertion position
    /// </summary>
    [JsonProperty(PropertyName = "lon", Required = Required.Always)]
    public double Lon { get; }

    /// <summary>
    /// Text rotation expressed as a survey angle (north is 0, increasing clockwise), in decimal degrees.
    /// </summary>
    [JsonProperty(PropertyName = "rot", Required = Required.Always)]
    public double Rot { get; }
    public AlignmentGeometryResultLabel(double stn, double lat, double lon, double rot)
    {
      Stn = stn;
      Lat = lat;
      Lon = lon;
      Rot = rot;
    }
  }
}

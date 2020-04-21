using Newtonsoft.Json;

namespace VSS.Productivity3D.Project.Abstractions.Models
{
  public class TBCPoint
  {
    /// <summary>
    /// Gets or sets the latitude
    /// </summary>
    [JsonProperty(PropertyName = "Latitude", Required = Required.Always)]
    public double Latitude { get; set; }
    /// <summary>
    /// Gets or sets the longitude
    /// </summary>
    [JsonProperty(PropertyName = "Longitude", Required = Required.Always)]
    public double Longitude { get; set; }

    public TBCPoint(double latitude, double longitude)
    {
      Latitude = latitude;
      Longitude = longitude;
    }
  }
}



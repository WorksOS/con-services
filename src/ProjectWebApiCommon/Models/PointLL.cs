using Newtonsoft.Json;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class PointLL
  {
    /// <summary>
    /// Gets or sets the latitude
    /// </summary>
    [JsonProperty(PropertyName = "latitude", Required = Required.Always)]
    public double latitude { get; set; }
    /// <summary>
    /// Gets or sets the longitude
    /// </summary>
    [JsonProperty(PropertyName = "longitude", Required = Required.Always)]
    public double longitude { get; set; }

    public PointLL(double latitude, double longitude)
    {
      this.latitude = latitude;
      this.longitude = longitude;
    }
  }
}

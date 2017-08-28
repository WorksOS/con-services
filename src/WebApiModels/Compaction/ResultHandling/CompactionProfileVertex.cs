using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// The data for a compaction design profile. 
  /// </summary>
  public class CompactionProfileVertex
  {
    /// <summary>
    /// The station value, or distance from start of the profile line at which the profile line intersects the design surface.
    /// </summary>
    [JsonProperty(PropertyName = "x", Required = Required.Always)]
    public double station;

    /// <summary>
    /// Elevation of the profile vertex.
    /// </summary>
    [JsonProperty(PropertyName = "y", Required = Required.Always)]
    public float elevation;
  }
}
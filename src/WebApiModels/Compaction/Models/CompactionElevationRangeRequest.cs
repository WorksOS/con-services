using Newtonsoft.Json;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// The representation of a elevation statistics request
  /// </summary>
  public class CompactionElevationRangeRequest : ProjectID
  {
    /// <summary>
    /// The filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public CompactionFilter filter { get; private set; }
  }
}
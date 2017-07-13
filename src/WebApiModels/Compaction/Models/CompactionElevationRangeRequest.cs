using Newtonsoft.Json;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApiModels.Compaction.Models
{
  /// <summary>
  /// The representation of a elevation statistics request
  /// </summary>
  public class CompactionElevationRangeRequest : ProjectID, IValidatable
  {
    /// <summary>
    /// The filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public CompactionFilter filter { get; private set; }
  }
}
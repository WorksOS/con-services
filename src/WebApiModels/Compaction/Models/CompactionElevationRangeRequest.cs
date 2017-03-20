

using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiModels.Compaction.Models
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

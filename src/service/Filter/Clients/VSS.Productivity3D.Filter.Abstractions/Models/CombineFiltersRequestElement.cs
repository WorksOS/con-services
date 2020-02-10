using Newtonsoft.Json;

namespace VSS.Productivity3D.Filter.Abstractions.Models
{
  public class HierarchicFilterElement
  {
    /// <summary>
    /// The UIDs of a filter is to be combined with other filters listed in the request
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string FilterUid { get; set; } = string.Empty;

    /// <summary>
    /// The role the filter assumes in the combination with other filters listed in the request
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public FilterCombinationRole Role{ get; set; } = FilterCombinationRole.Undefined;
  }
}

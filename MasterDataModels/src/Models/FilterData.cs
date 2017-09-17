using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
  public class FilterData : BaseDataResult
  {
    /// <summary>
    /// Gets or sets the filter descriptor.
    /// </summary>
    /// <value>
    /// The filter descriptor.
    /// </value>
    [JsonProperty(PropertyName = "filterDescriptor")]
    public FilterDescriptor filterDescriptor { get; set; }
  }
}
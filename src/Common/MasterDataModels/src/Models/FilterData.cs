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
    public FilterDescriptor filterDescriptor { get; set; }
  }
}
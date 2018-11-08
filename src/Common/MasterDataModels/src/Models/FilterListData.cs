using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
  public class FilterListData : BaseDataResult
  {
    /// <summary>
    /// Gets or sets the filter descriptors.
    /// </summary>
    /// <value>
    /// The filter descriptors.
    /// </value>
    public List<FilterDescriptor> filterDescriptors { get; set; }
  }
}

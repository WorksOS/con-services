using System.Collections.Generic;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  public class FilterDataResult : BaseDataResult
  {
    /// <summary>
    /// Gets or sets the filter descriptors.
    /// </summary>
    /// <value>
    /// The project descriptors.
    /// </value>
    public List<FilterData> FilterDescriptors { get; set; }
  }
}

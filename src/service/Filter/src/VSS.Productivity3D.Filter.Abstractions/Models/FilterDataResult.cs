using System.Collections.Generic;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Abstractions.Models
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

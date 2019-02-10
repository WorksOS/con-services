using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Abstractions.Models
{
  public class FilterData : BaseDataResult, IMasterDataModel
  {
    /// <summary>
    /// Gets or sets the filter descriptor.
    /// </summary>
    /// <value>
    /// The filter descriptor.
    /// </value>
    public FilterDescriptor filterDescriptor { get; set; }

    public List<string> GetIdentifiers() => filterDescriptor?.GetIdentifiers() ?? new List<string>();
  }
}

using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.MasterData.Models.Models
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
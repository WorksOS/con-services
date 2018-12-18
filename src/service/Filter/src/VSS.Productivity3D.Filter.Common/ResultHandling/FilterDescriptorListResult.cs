using System.Collections.Immutable;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Filter.Common.ResultHandling
{
  /// <summary>
  /// Single/List of filters returned from endpoint
  /// </summary>
  public class FilterDescriptorListResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets or sets the filter descriptors
    /// </summary>
    public ImmutableList<FilterDescriptor> FilterDescriptors { get; set; }
  }
}

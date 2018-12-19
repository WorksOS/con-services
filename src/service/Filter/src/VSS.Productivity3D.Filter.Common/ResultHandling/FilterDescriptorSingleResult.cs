using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Filter.Common.ResultHandling
{
  /// <summary>
  /// Single filter descriptor
  /// </summary>
  public class FilterDescriptorSingleResult : ContractExecutionResult
  {
    public FilterDescriptorSingleResult(FilterDescriptor filterDescriptor)
    {
      FilterDescriptor = filterDescriptor;
    }

    /// <summary>
    /// Gets or sets the filter descriptor.
    /// </summary>
    public FilterDescriptor FilterDescriptor { get; set; }
  }
}

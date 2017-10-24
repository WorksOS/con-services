using VSS.Common.ResultsHandling;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Common.ResultHandling
{
  /// <summary>
  ///   Single filter descriptor
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
    /// <value>
    /// The filter descriptor.
    /// </value>
    public FilterDescriptor FilterDescriptor { get; set; }
  }
}
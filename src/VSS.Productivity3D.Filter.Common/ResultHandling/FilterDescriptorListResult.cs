using System.Collections.Immutable;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Common.ResultHandling
{
  /// <summary>
  /// Single/List of filters returned from endpoint
  /// </summary>
  /// <seealso cref="ContractExecutionResult" />
  public class FilterDescriptorListResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets or sets the filter descriptors
    /// </summary>
    /// <value>
    /// The filter descriptors.
    /// </value>
    public ImmutableList<FilterDescriptor> FilterDescriptors { get; set; }
  }
}
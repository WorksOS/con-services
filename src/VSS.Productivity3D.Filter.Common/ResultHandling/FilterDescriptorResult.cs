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
    public ImmutableList<FilterDescriptor> filterDescriptors { get; set; }
  }


  /// <summary>
  ///   Single filter descriptor
  /// </summary>
  public class FilterDescriptorSingleResult : ContractExecutionResult
  {
    private FilterDescriptor _filterDescriptor;

    public FilterDescriptorSingleResult(FilterDescriptor filterDescriptor)
    {
      this.filterDescriptor = filterDescriptor;
    }

    /// <summary>
    /// Gets or sets the filter descriptor.
    /// </summary>
    /// <value>
    /// The filter descriptor.
    /// </value>
    public FilterDescriptor filterDescriptor
    {
      get { return _filterDescriptor; }
      set { _filterDescriptor = value; }
    }
  }
}

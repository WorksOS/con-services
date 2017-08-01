using System.Collections.Immutable;

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
    public FilterDescriptor filterDescriptor { get { return _filterDescriptor; } set { _filterDescriptor = value; } }
  }


  /// <summary>
  ///   Describes a filter
  /// </summary>
  public class FilterDescriptor
  {
    // CustomerUid; UserUid and projectUid must be provided so shouldn't be returned
    
    /// <summary>
    /// Gets or sets the filter uid.
    /// </summary>
    /// <value>
    /// The filter uid.
    /// </value>
    public string FilterUid { get; set; }

    /// <summary>
    /// Gets or sets the name of the filter.
    ///    if empty, then this is a transient filter
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the string containing the filter in Json format
    /// </summary>
    /// <value>
    /// The FilterJson.
    /// </value>
    public string FilterJson { get; set; }


    public override bool Equals(object obj)
    {
      var otherFilter = obj as FilterDescriptor;
      if (otherFilter == null) return false;
      return otherFilter.FilterUid == this.FilterUid
             && otherFilter.Name == this.Name
             && otherFilter.FilterJson == this.FilterJson
        ;
    }

    public override int GetHashCode() { return 0; }
  }
}

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
  public class FilterData : BaseDataResult
  {

    /// <summary>
    /// Gets or sets the filter descriptor.
    /// </summary>
    /// <value>
    /// The filter descriptor.
    /// </value>
    [JsonProperty(PropertyName = "filterDescriptor")]
    public FilterDescriptor filterDescriptor { get; set; }

  }

  /// <summary>
  ///   Describes a filter
  /// </summary>
  /// CustomerUid; UserUid and projectUid must be provided so shouldn't be returned

  public class FilterDescriptor
  {
    /// <summary>
    /// Gets or sets the filter uid.
    /// </summary>
    /// <value>
    /// The filter uid.
    /// </value>
    [JsonProperty(PropertyName = "FilterUid")]
    public string FilterUid { get; set; }

    /// <summary>
    /// Gets or sets the name of the filter.
    ///    if empty, then this is a transient filter
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    [JsonProperty(PropertyName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the string containing the filter in Json format
    /// </summary>
    /// <value>
    /// The FilterJson.
    /// </value>
    [JsonProperty(PropertyName = "FilterJson")]
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

    public override int GetHashCode()
    {
      return 0;
    }
  }
}

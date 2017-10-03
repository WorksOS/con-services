using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// Describes a filter
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
    [JsonProperty(PropertyName = "filterUid")]
    public string FilterUid { get; set; }

    /// <summary>
    /// Gets or sets the name of the filter.
    /// If empty, then this is a transient filter
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the string containing the filter in Json format
    /// </summary>
    /// <value>
    /// The FilterJson.
    /// </value>
    [JsonProperty(PropertyName = "filterJson")]
    public string FilterJson { get; set; }

    public override bool Equals(object obj)
    {
      var otherFilter = obj as FilterDescriptor;
      if (otherFilter == null)
      {
        return false;
      }

      return otherFilter.FilterUid == FilterUid
             && otherFilter.Name == Name
             && otherFilter.FilterJson == FilterJson;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}
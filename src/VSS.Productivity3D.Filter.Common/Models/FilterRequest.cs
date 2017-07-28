using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Filter.Common.Models
{
  public class FilterRequest
  {
    /// <summary>
    /// The projectUid whose filter is to be upserted
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Always)]
    public string projectUid { get; set; }

    /// <summary>
    /// The filterUid whose filter is to be udpated, empty for create
    /// </summary>
    [JsonProperty(PropertyName = "filterUid", Required = Required.Default)]
    public string filterUid { get; set; }

    /// <summary>
    /// The name to be upserted, if empty then filter is transient
    /// </summary>
    [JsonProperty(PropertyName = "name", Required = Required.Always)]
    public string name { get; set; }

    /// <summary>
    /// The filter containing the Json string. May be empty if all defaults
    /// </summary>
    [JsonProperty(PropertyName = "FilterJson", Required = Required.Always)]
    public string filterJson { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private FilterRequest()
    {
    }

    /// <summary>
    /// Create instance of FilterRequest
    /// </summary>
    public static FilterRequest CreateFilterRequest(string projectUid, string name, string filterJson)
    {
      return new FilterRequest
      {
        projectUid = projectUid,
        name = name,
        filterJson = filterJson
      };
    }
  }
}

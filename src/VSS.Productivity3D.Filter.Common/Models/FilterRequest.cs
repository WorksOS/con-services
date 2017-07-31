using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Filter.Common.Models
{
  public class FilterRequest
  {
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
    protected FilterRequest()
    {
    }

    /// <summary>
    /// Create instance of FilterRequest
    /// </summary>
    public static FilterRequest CreateFilterRequest(string name, string filterJson)
    {
      return new FilterRequest
      {
        name = name,
        filterJson = filterJson
      };
    }
  }

  public class FilterRequestFull : FilterRequest
  {
    public string customerUid { get; set; }

    public string userUid { get; set; }
    public string projectUid { get; set; }

    public bool isApplicationContext { get; set; }

    public static FilterRequestFull CreateFilterFullRequest(string customerUid, 
      bool isApplicationContext, string userUid, 
      string projectUid, string filterUid = null,
      string name = null, string filterJson = null)
    {
      return new FilterRequestFull
      {
        projectUid = projectUid,
        filterUid = filterUid,
        name = name,
        filterJson = filterJson,
        customerUid = customerUid,
        isApplicationContext = isApplicationContext,
        userUid = userUid
      };
    }
  }
}

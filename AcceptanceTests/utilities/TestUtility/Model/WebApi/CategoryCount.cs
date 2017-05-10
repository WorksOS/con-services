using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestUtility.Model.WebApi
{
  public class CategoryCount
  {
    /// <summary>
    /// Asset count detail i.e. what the count represents.
    /// </summary>
    [JsonProperty(PropertyName = "countOf")]
    public string CountOf { get; set; }
    /// <summary>
    /// Asset count.
    /// </summary>
    [JsonProperty(PropertyName = "count")]
    public int Count { get; set; }
  }
}

using System;
using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class LastDirChangeResult : ApiResult
  {
    [JsonProperty(PropertyName = "lastUpdatedDateTime", Required = Required.Default)]
    public DateTime lastUpdatedDateTime;
  }
}
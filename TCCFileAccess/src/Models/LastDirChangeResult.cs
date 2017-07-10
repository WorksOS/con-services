using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.TCCFileAccess.Models
{
  public class LastDirChangeResult : ApiResult
  {
    [JsonProperty(PropertyName = "lastUpdatedDateTime", Required = Required.Default)]
    public DateTime lastUpdatedDateTime;
  }
}

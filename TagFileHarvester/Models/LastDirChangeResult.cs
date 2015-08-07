using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class LastDirChangeResult : ApiResult
  {
    [JsonProperty(PropertyName = "lastUpdatedDateTime", Required = Required.Default)]
    public DateTime lastUpdatedDateTime;
  }
}

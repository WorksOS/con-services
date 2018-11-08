using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VSS.Productivity3D.TagFileHarvester.Models
{
  public class GetFileSpacesResult : ApiResult
  {
    [JsonProperty(PropertyName = "filespaces", Required = Required.Default)] 
    public FileSpace[] filespaces;
  }

  public class FileSpace
  {
   [JsonProperty(PropertyName = "filespaceId", Required = Required.Default)]
    public string filespaceId;
    [JsonProperty(PropertyName = "orgDisplayName", Required = Required.Default)]
    public string orgDisplayName;
    [JsonProperty(PropertyName = "orgId", Required = Required.Default)]
    public string orgId;
    [JsonProperty(PropertyName = "orgShortname", Required = Required.Default)]
    public string orgShortname;
    [JsonProperty(PropertyName = "shortname", Required = Required.Default)]
    public string shortname;
    [JsonProperty(PropertyName = "title", Required = Required.Default)]
    public string title;
  }
}

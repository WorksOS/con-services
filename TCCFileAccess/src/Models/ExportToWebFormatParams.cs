
using Newtonsoft.Json;

namespace VSS.TCCFileAccess.Models
{
  public class ExportToWebFormatParams
  {
    [JsonProperty(PropertyName = "sourcefilespaceid", Required = Required.Always)]
    public string sourcefilespaceid;
    [JsonProperty(PropertyName = "sourcepath", Required = Required.Always)]
    public string sourcepath;
    [JsonProperty(PropertyName = "destfilespaceid", Required = Required.Always)]
    public string destfilespaceid;
    [JsonProperty(PropertyName = "destpath", Required = Required.Always)]
    public string destpath;
    [JsonProperty(PropertyName = "format", Required = Required.Always)]
    public string format;
    [JsonProperty(PropertyName = "numzoomlevels", Required = Required.Always)]
    public int numzoomlevels;
    [JsonProperty(PropertyName = "maxzoomlevel", Required = Required.Always)]
    public int maxzoomlevel;
    [JsonProperty(PropertyName = "imageformat", Required = Required.Always)]
    public string imageformat;
  }
}

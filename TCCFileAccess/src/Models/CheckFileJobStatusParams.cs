using Newtonsoft.Json;

namespace VSS.Productivity3D.TCCFileAccess.Models
{
  public class CheckFileJobStatusParams
  {
    [JsonProperty(PropertyName = "jobid", Required = Required.Always)]
    public string jobid;
  }
}

using Newtonsoft.Json;

namespace TCCFileAccess.Models
{
  public class CheckFileJobStatusParams
  {
    [JsonProperty(PropertyName = "jobid", Required = Required.Always)]
    public string jobid;
  }
}

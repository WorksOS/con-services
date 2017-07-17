using Newtonsoft.Json;

namespace VSS.TCCFileAccess.Models
{
  public class CheckExportJobParams 
  {
    [JsonProperty(PropertyName = "jobid", Required = Required.Always)]
    public string jobid;
  }
}

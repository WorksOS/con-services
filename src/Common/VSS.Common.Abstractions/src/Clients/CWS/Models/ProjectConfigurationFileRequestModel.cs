using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class ProjectConfigurationFileRequestModel
  {
    /// <summary>
    /// DataOcean filespaceId
    /// </summary>
    [JsonProperty("machineControlFilespaceId")]
    public string MachineControlFilespaceId { get; set; }
  }   
}

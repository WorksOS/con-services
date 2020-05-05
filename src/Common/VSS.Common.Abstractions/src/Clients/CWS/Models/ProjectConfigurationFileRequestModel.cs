using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class ProjectConfigurationFileRequestModel
  {
    /// <summary>
    /// DataOcean filespaceId for machineControllers
    /// </summary>
    [JsonProperty("machineControlFilespaceId")]
    public string MachineControlFilespaceId { get; set; }

    /// <summary>
    /// DataOcean filespaceId for siteControllers
    /// </summary>
    [JsonProperty("siteCollectorFilespaceId")]
    public string SiteCollectorFilespaceId { get; set; }

  }
}

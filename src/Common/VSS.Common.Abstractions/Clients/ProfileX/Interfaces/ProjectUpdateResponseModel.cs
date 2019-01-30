using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.ProfileX.Interfaces
{
  public class ProjectUpdateResponseModel
  {
    /// <summary>
    /// Project TRN ID
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
  }
}
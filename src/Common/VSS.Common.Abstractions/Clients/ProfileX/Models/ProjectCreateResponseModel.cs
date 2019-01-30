using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.ProfileX.Models
{
  public class ProjectCreateResponseModel
  {
    /// <summary>
    /// Project TRN ID
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
  }
}
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.ProfileX.Models
{
  public class ProjectExternalReferencesResponse
  {
    /// <summary>
    /// This is a response, so we can't create one directly
    /// </summary>
    [JsonConstructor]
    private ProjectExternalReferencesResponse()
    {
      
    }
    /// <summary>
    /// Key for the project references
    /// </summary>
    [JsonProperty("id", Required = Required.Always)]
    public string Id { get; set; }

    /// <summary>
    /// Key for the project references
    /// </summary>
    [JsonProperty("key", Required = Required.Always)]
    public string Key { get; set; }

    [JsonProperty("references", Required = Required.Always)]
    public List<Reference> References { get; set; }
  }
}
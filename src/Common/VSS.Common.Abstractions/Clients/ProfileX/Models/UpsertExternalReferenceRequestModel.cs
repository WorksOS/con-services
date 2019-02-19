using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.ProfileX.Models
{
  /// <summary>
  /// Used in create or update project References
  /// The same model is used for both.
  /// </summary>
  public class UpsertExternalReferenceRequestModel
  {
    public UpsertExternalReferenceRequestModel()
    {
      References = new List<Reference>();
    }

    /// <summary>
    /// Key for the project references
    /// </summary>
    [JsonProperty("key", Required = Required.Always)]
    public string Key { get; set; }

    [JsonProperty("references", Required = Required.Always)]
    public List<Reference> References { get; set; }
  }
}
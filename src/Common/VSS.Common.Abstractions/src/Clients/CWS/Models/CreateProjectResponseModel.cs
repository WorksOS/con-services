using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class CreateProjectResponseModel : IMasterDataModel
  {
    /// <summary>
    /// Project TRN ID
    /// </summary>
    [JsonProperty("projectId")]
    public string Id { get; set; }

    public List<string> GetIdentifiers() => new List<string> { Id };
  }

  /* example
    {
      "projectId": "trn::profilex:us-west-2:project:815b84bf-13c7-43dd-b80e-3f36at"
    }
   */
}

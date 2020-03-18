using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class CreateProjectResponseModel
  {
    
    /// <summary>
    /// Project TRN ID
    /// </summary>
    [JsonProperty("projectId")]
    public string Id { get; set; }
  }

  /* example
    {
      "projectId": "trn::profilex:us-west-2:project:815b84bf-13c7-43dd-b80e-3f36at"
    }
   */
}

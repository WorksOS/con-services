using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class UpdateProjectDetailsRequestModel
  {
    /// <summary>
    /// Project name
    /// </summary>
    [JsonProperty("projectName")]
    public string projectName { get; set; }

  }

  /* example
    {
      "projectName": "{{projectName}}"      
    }
  */
}

using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  // CCSSSCON-23 this doesn't include start and end dates or description
  //  marketing are still going back and forth on these
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

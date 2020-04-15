using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  // todoMaverick this doesn't include start and end dates. 
  //  Steve says this is as expected.
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

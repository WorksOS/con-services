using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class ProjectListResponseModel : IMasterDataModel
  {
    public ProjectListResponseModel()
    {
      Projects = new List<ProjectResponseModel>();
    }

    /// <summary>
    /// Projects
    /// </summary>
    [JsonProperty("projects")]
    public List<ProjectResponseModel> Projects { get; set; }

    /// <summary>
    /// Returned as true if the result has more records to display. Helps in pagination. False implies that there are no more records to display.
    /// </summary>
    [JsonProperty("hasMore")]
    public bool HasMore { get; set; }

    public List<string> GetIdentifiers() => Projects?
                                              .SelectMany(d => d.GetIdentifiers())
                                              .Distinct()
                                              .ToList()
                                            ?? new List<string>();
  }

  public class ProjectResponseModel : IMasterDataModel
  {
    //Note: There are other properties returned but we only want some of it

    /// <summary>
    /// account TRN ID
    /// </summary>
    [JsonProperty("accountId")]
    public string accountId { get; set; }

    /// <summary>
    /// project TRN ID
    /// </summary>
    [JsonProperty("projectId")]
    public string projectId { get; set; }

    /// <summary>
    /// Project name
    /// </summary>
    [JsonProperty("projectName")]
    public string projectName { get; set; }

    /// <summary>
    /// cws example = "America/Denver"
    /// </summary>
    [JsonProperty("timezone")]
    public string timezone { get; set; }

    /// <summary>
    /// 3dp supports what types?
    /// </summary>
    [JsonProperty("boundary")]
    public ProjectBoundary boundary { get; set; }

    public List<string> GetIdentifiers() => 
      new List<string>   { projectId };

  }
}

  /* example
    {
      "accountId": "{{accountId}}",
      "projectId": "{{projectId}}",
      "projectName": "{{projectName}}",
      "timezone": "America/Denver",
      "boundary": {
        "type": "Polygon",
        "coordinates": [
            [
                [
                    -105.115560734865,
                    39.898797315920504
                ],
                [
                    -105.11758904248114,
                    39.89642626198623
                ],
                [
                    -105.1139381054785,
                    39.894661780621746
                ],
                [
                    -105.11212447489478,
                    39.8973452453979
                ],
                [
                    -105.115560734865,
                    39.898797315920504
                ]
            ]
        ]
    }
  */

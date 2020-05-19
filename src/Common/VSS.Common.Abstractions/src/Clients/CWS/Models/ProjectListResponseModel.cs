using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
    private string _accountTrn;
    private string _projectTrn;

    //Note: There are other properties returned but we only want some of it

    /// <summary>
    /// account TRN ID
    /// </summary>
    [JsonProperty("accountId")]
    public string AccountTRN
    {
      get => _accountTrn;
      set
      {
        _accountTrn = value;
        AccountId = TRNHelper.ExtractGuidAsString(value);
      }
    }

    /// <summary>
    /// WorksOS account ID; the Guid extracted from the TRN.
    /// </summary>}
    public string AccountId { get; private set; }

    /// <summary>
    /// project TRN ID
    /// </summary>
    [JsonProperty("projectId")]
    public string ProjectTRN
    {
      get => _projectTrn;
      set
      {
        _projectTrn = value;
        ProjectId = TRNHelper.ExtractGuidAsString(value);
      }
    }

    /// <summary>
    /// WorksOS project ID; the Guid extracted from the TRN.
    /// </summary>
    public string ProjectId { get; private set; }

    /// <summary>
    /// Project name
    /// </summary>
    [JsonProperty("projectName")]
    public string ProjectName { get; set; }

    /// <summary>
    /// cws example = "America/Denver"
    /// </summary>
    [JsonProperty("timezone")]
    public string Timezone { get; set; }

    /// <summary>
    /// 3dp supports what types?
    /// </summary>
    [JsonProperty("boundary")]
    public ProjectBoundary Boundary { get; set; }

    public List<string> GetIdentifiers() => 
      new List<string>   { ProjectTRN, ProjectId };

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

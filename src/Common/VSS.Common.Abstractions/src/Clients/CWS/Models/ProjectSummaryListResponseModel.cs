using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Enums;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class ProjectSummaryListResponseModel : IMasterDataModel
  {
    public ProjectSummaryListResponseModel()
    {
      Projects = new List<ProjectSummaryResponseModel>();
    }

    /// <summary>
    /// Projects
    /// </summary>
    [JsonProperty("projects")]
    public List<ProjectSummaryResponseModel> Projects { get; set; }

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

  public class ProjectSummaryResponseModel : IMasterDataModel
  {
    private string _projectTrn;
    //Note: There are other properties returned but we only want some of it
    //      it includes boundary and timezone but these are always null in the getProjects for user/account,
    //      we need to do a subsequent get on each project to obtain these details. 
    //      it includes user email (only) and createdAt date in case we ever need these

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
    /// WorksOS device ID; the Guid extracted from the TRN.
    /// </summary>
    [JsonProperty("projectUid")]
    public string ProjectId { get; set; }

    /// <summary>
    /// Project name
    /// </summary>
    [JsonProperty("projectName")]
    public string ProjectName { get; set; }

    /// <summary>
    /// role "ADMIN" only one at present where user has access to project
    ///   possible others are null and Pending?
    /// </summary>
    [JsonConverter(typeof(NullableEnumStringConverter), UserProjectRoleEnum.Unknown)]
    [JsonProperty("role")]
    public UserProjectRoleEnum UserProjectRole { get; set; }

    /// <summary>
    /// project boundary
    /// </summary>
    [JsonProperty("boundary")]
    public ProjectBoundary Boundary { get; set; }

    /// <summary>
    /// time zone in WM format e.g. "Pacific/Auckland"
    /// </summary>
    [JsonProperty("timezone")]
    public string TimeZone { get; set; }

    /// <summary>
    /// Project type. Todo change to enum once all the cws work is done
    /// </summary>
    public int ProjectType { get; set; }


    public List<string> GetIdentifiers() => new List<string> { ProjectTRN, ProjectId };

  }
}

/* example
 {
  "hasMore": true,
  "projects": [
      {
          "projectId": "trn::profilex:us-west-2:project:3f7a1bd9-0072-436b-81a5-91cc5c5d6057",
          "projectName": "jeannie Test project 2",
          "role": "ADMIN",
          "userCount": 1,
          "deviceCount": 0,
          "timezone": null,
          "boundary": null,
          "boundaryCentroid": null,
          "boundaryCentroidTimezone": null,
          "boundaryArea": null,
          "createdBy": {
              "role": null,
              "lifeStatus": null,
              "userId": null,
              "email": "jeannie_may@trimble.com",
              "firstName": null,
              "lastName": null,
              "phone": null,
              "profileImage": null
          },
          "createdAt": "2020-05-11T20:19:03Z"
      },
      {
          "projectId": "trn::profilex:us-west-2:project:2b2e13bd-4621-419c-b179-68c8df867d45",
          "projectName": "Test Project Creation",
          "role": null,
          "userCount": 1,
          "deviceCount": 0,
          "timezone": null,
          "boundary": null,
          "boundaryCentroid": null,
          "boundaryCentroidTimezone": null,
          "boundaryArea": null,
          "createdBy": {
              "role": null,
              "lifeStatus": null,
              "userId": null,
              "email": "priyanga_jayaraman+stg@trimble.com",
              "firstName": null,
              "lastName": null,
              "phone": null,
              "profileImage": null
          },
          "createdAt": "2020-05-11T04:15:55Z"
      }
  ]
}
*/

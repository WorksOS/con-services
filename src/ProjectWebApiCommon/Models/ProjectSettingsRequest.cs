using System.Net;
using Newtonsoft.Json;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class ProjectSettingsRequest : ProjectUID
  {
    /// <summary>
    /// The settings to be CRUD
    /// </summary>
    [JsonProperty(PropertyName = "settings", Required = Required.Always)]
    public string settings { get; set; }

    /// <summary>
    /// The type of project settings
    /// </summary>
    [JsonIgnore]//So existing contract is not broken
    public ProjectSettingsType projectSettingsType { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ProjectSettingsRequest()
    {
    }

    /// <summary>
    /// Create instance of ProjectSettingsRequest
    /// </summary>
    public static ProjectSettingsRequest CreateProjectSettingsRequest(string projectUid, string settings, ProjectSettingsType projectSettingsType)
    {
      return new ProjectSettingsRequest
      {
        projectUid = projectUid,
        settings = settings,
        projectSettingsType = projectSettingsType
      };
    }

    public override void Validate()
    {
      base.Validate();
      if (settings == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2073, "ProjectSettings cannot be null."));
      }

    }
  }
}

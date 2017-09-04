using System.Net;
using Newtonsoft.Json;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;

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
    /// Private constructor
    /// </summary>
    private ProjectSettingsRequest()
    {
    }

    /// <summary>
    /// Create instance of ProjectSettingsRequest
    /// </summary>
    public static ProjectSettingsRequest CreateProjectSettingsRequest(string projectUid, string settings)
    {
      return new ProjectSettingsRequest
      {
        projectUid = projectUid,
        settings = settings
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

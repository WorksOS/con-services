using Newtonsoft.Json;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Models.Models
{
  public class ProjectSettingsRequest : ProjectUID
  {
    /// <summary>
    /// The settings to be CRUD
    /// </summary>
    [JsonProperty(PropertyName = "settings", Required = Required.Always)]
    public string Settings { get; set; }

    /// <summary>
    /// The type of project settings
    /// </summary>
    ///[JsonIgnore]//So existing contract is not broken
    [JsonProperty(PropertyName = "projectSettingsType", Required = Required.Default)]
    public ProjectSettingsType ProjectSettingsType { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ProjectSettingsRequest()
    { }

    /// <summary>
    /// Create instance of ProjectSettingsRequest
    /// </summary>
    public static ProjectSettingsRequest CreateProjectSettingsRequest(string projectUid, string settings, ProjectSettingsType projectSettingsType)
    {
      return new ProjectSettingsRequest
      {
        projectUid = projectUid,
        Settings = settings,
        ProjectSettingsType = projectSettingsType
      };
    }

    public override void Validate()
    {
      base.Validate();
      if (Settings == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2073, "ProjectSettings cannot be null."));
      }
    }
  }
}
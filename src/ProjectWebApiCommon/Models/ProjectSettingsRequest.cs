using Newtonsoft.Json;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class ProjectSettingsRequest
  {
    /// <summary>
    /// The id of the projectUid whose settings are to be upserted
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Always)]
    public string projectUid { get; set; }

    /// <summary>
    /// The settings to be upserted
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
    /// Create instance of GetProjectIdRequest
    /// </summary>
    public static ProjectSettingsRequest CreateProjectSettingsRequest(string projectUid, string settings)
    {
      return new ProjectSettingsRequest
      {
        projectUid = projectUid,
        settings = settings
      };
    }
  }
}

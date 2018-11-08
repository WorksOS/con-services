using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.ResultsHandling
{
  public class ProjectSettingsResult : ContractExecutionResult
  {
    /// <summary>
    /// The projectUid
    /// </summary>
    [JsonProperty(PropertyName = "projectUid")]
    public string projectUid { get;  set; }

    /// <summary>
    /// The projects settings
    /// </summary>
    [JsonProperty(PropertyName = "settings")]
    public JObject settings { get; set; }

    /// <summary>
    /// The type of project settings
    /// </summary>
    [JsonIgnore]//So existing contract is not broken
    public ProjectSettingsType projectSettingsType { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ProjectSettingsResult()
    { }


    /// <summary>
    /// ProjectSettingsResult create instance
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="settings"></param>
    /// <param name="projectSettingsType"></param>
    /// <returns></returns>
    public static ProjectSettingsResult CreateProjectSettingsResult(
      string projectUid, JObject settings, ProjectSettingsType projectSettingsType)
    {
      return new ProjectSettingsResult
      {
        projectUid = projectUid,
        settings = settings,
        projectSettingsType = projectSettingsType
      };
    }
    
  }
}

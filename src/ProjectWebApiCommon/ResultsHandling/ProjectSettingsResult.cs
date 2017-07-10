using Newtonsoft.Json;

namespace VSS.Productivity3D.ProjectWebApiCommon.ResultsHandling
{
  public class ProjectSettingsResult : ContractExecutionResult
  {
    /// <summary>
    /// The projectUid
    /// </summary>
    [JsonProperty(PropertyName = "projectUid")]
    public string ProjectUid { get;  set; }

    /// <summary>
    /// The projects settings
    /// </summary>
    [JsonProperty(PropertyName = "projectsettings")]
    public string Settings { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ProjectSettingsResult()
    { }


    /// <summary>
    /// ProjectSettingsResult create instance
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="projectSettings"></param>
    /// <returns></returns>
    public static ProjectSettingsResult CreateProjectSettingsResult(string projectUid,
      string projectSettings)
    {
      return new ProjectSettingsResult
      {
        ProjectUid = projectUid,
        Settings = projectSettings
      };
    }
    
  }
}

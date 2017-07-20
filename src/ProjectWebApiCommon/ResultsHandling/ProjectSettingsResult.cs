using Newtonsoft.Json;

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
    public string settings { get; set; }

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
    /// <returns></returns>
    public static ProjectSettingsResult CreateProjectSettingsResult(string projectUid,
      string settings)
    {
      return new ProjectSettingsResult
      {
        projectUid = projectUid,
        settings = settings
      };
    }
    
  }
}

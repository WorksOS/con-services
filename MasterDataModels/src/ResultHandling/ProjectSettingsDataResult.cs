using Newtonsoft.Json;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  public class ProjectSettingsDataResult : BaseDataResult
  {
    /// <summary>
    /// The projectUid
    /// </summary>
    [JsonProperty(PropertyName = "projectUid")]
    public string ProjectUid { get; set; }

    /// <summary>
    /// The projects settings
    /// </summary>
    [JsonProperty(PropertyName = "projectsettings")]
    public string Settings { get; set; }

  }
}

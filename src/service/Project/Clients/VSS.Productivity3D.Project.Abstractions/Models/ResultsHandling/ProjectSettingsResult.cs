using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  public class ProjectSettingsResult : ContractExecutionResult, IMasterDataModel
  {
    /// <summary>
    /// The projectUid
    /// </summary>
    [JsonProperty(PropertyName = "projectUid")]
    public string ProjectUid { get;  set; }

    /// <summary>
    /// The projects settings
    /// </summary>
    [JsonProperty(PropertyName = "settings")]
    public JObject Settings { get; set; }

    /// <summary>
    /// The type of project settings
    /// </summary>
    [JsonIgnore]//So existing contract is not broken
    public ProjectSettingsType ProjectSettingsType { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public ProjectSettingsResult()
    { }


    /// <summary>
    /// Constructor with parameters
    /// </summary>
    public ProjectSettingsResult(
      string projectUid, JObject settings, ProjectSettingsType projectSettingsType)
    {
      ProjectUid = projectUid;
      ProjectSettingsType = projectSettingsType;
      Settings = settings;
    }

    public List<string> GetIdentifiers() => new List<string>()
    {
      ProjectUid
    };

  }
}

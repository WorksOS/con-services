
using Newtonsoft.Json.Linq;
using RaptorSvcAcceptTestsCommon.Models;

namespace VSS.MasterData.Models.Models
{
  public class ProjectSettingsRequest
  {
    /// <summary>
    /// The id of the projectUid whose settings are to be upserted
    /// </summary>
    public string projectUid { get; set; }

    /// <summary>
    /// The settings to be CRUD
    /// </summary>
    public string Settings { get; set; }

    /// <summary>
    /// The type of project settings
    /// </summary>
    public int ProjectSettingsType { get; set; }
  }
}
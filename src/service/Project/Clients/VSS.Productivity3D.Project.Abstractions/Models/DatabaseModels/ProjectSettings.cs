using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels
{
  public class ProjectSettings
  {
    public string ProjectUid { get; set; }
    public ProjectSettingsType ProjectSettingsType { get; set; }
    public string Settings { get; set; }

    // UserID will include either a UserUID (GUID) or ApplicationID (string)
    public string UserID { get; set; }

    public DateTime LastActionedUtc { get; set; }
  }
}

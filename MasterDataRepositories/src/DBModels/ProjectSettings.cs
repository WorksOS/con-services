using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Repositories.DBModels
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
using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
{
  public class UpdateProjectSettingsEvent : IProjectEvent
  {
    public string ProjectUID { get; set; }
    public ProjectSettingsType ProjectSettingsType { get; set; }
    public string Settings { get; set; }

    // UserID will include either a UserUID (GUID) or ApplicationID (string)
    public string UserID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}

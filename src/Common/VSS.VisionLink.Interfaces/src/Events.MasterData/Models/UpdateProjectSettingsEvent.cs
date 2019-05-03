using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;


namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class UpdateProjectSettingsEvent : IProjectEvent
  {
    public Guid ProjectUID { get; set; }
    public ProjectSettingsType ProjectSettingsType { get; set; }
    public string Settings { get; set; }

    // UserID will include either a UserUID (GUID) or ApplicationID (string)
    public string UserID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}

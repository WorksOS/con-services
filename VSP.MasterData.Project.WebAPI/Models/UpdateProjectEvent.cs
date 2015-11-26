using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Interfaces.Events.MasterData.Models
{
  public class UpdateProjectEvent : IProjectEvent
  {
    public DateTime ProjectEndDate { get; set; }
    public string ProjectTimezone { get; set; }
    public string ProjectName { get; set; }

    public Guid ProjectUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}
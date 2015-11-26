using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Interfaces.Events.MasterData.Models
{
  public class CreateProjectEvent : IProjectEvent
  {
    public DateTime ProjectEndDate { get; set; }
    public DateTime ProjectStartDate { get; set; }
    public string ProjectTimezone { get; set; }
    public string ProjectName { get; set; }
    public ProjectType ProjectType { get; set; }
    public string ProjectBoundaries { get; set; }


    public Guid ProjectUID { get; set; }
    public int ProjectID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }

}

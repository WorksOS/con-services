using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class CreateProjectEvent : IProjectEvent
  {
    public DateTime ProjectEndDate { get; set; }
    public DateTime ProjectStartDate { get; set; }
    public string ProjectTimezone { get; set; }
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public ProjectType ProjectType { get; set; }
    public string ProjectBoundary { get; set; }


    public Guid ProjectUID { get; set; }
    public Guid CustomerUID { get; set; }

    //CurrentGen only. Should not be used in NG as it should be assigned but PorjectMasterData automatically
    public int ProjectID { get; set; }
    public long CustomerID { get; set; }

    public string CoordinateSystemFileName { get; set; }
    public byte[] CoordinateSystemFileContent { get; set; }

    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }

}

using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
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


    public string ProjectUID { get; set; }
    public string CustomerUID { get; set; }

    //CurrentGen only. Should not be used in NG as it should be assigned but ProjectMasterData automatically
    public int ShortRaptorProjectId { get; set; }

    public string CoordinateSystemFileName { get; set; }
    public byte[] CoordinateSystemFileContent { get; set; }

    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }

}

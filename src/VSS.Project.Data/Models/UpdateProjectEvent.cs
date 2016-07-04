using System;
using VSS.Project.Data.Interfaces;

namespace VSS.Project.Data.Models
{
  public class UpdateProjectEvent : IProjectEvent
  {
    public DateTime ProjectEndDate { get; set; }
    public string ProjectTimezone { get; set; }
    public string ProjectName { get; set; }
    public ProjectType ProjectType { get; set; }

    public Guid ProjectUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}
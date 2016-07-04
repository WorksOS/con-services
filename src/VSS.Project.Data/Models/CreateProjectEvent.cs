using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Project.Data.Interfaces;

namespace VSS.Project.Data.Models
{
  public class CreateProjectEvent : IProjectEvent
  {
    public DateTime ProjectEndDate { get; set; }
    public DateTime ProjectStartDate { get; set; }
    public string ProjectTimezone { get; set; }
    public string ProjectName { get; set; }
    public ProjectType ProjectType { get; set; }
    public string ProjectBoundary { get; set; }


    public Guid ProjectUID { get; set; }
    public int ProjectID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}

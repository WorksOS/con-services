using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDAPIs.ProjectMasterData
{
  public class UpdateProjectEvent : IProjectEvent
  {
    public DateTime ProjectEndDate { get; set; }
    public string ProjectTimezone { get; set; }
    public string ProjectName { get; set; }
    public int ProjectType { get; set; }

    public Guid ProjectUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }

}

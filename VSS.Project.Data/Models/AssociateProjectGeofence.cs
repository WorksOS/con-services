using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Project.Data.Interfaces;

namespace VSS.Project.Data.Models
{
  public class AssociateProjectGeofence : IProjectEvent
  {
    public Guid ProjectUID { get; set; }
    public Guid GeofenceUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}

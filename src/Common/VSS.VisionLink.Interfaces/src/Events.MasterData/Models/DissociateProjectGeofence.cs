using System;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Events.MasterData.Models
{
  public class DissociateProjectGeofence : IProjectEvent
  {
    public Guid ProjectUID { get; set; }
    public Guid GeofenceUID { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}

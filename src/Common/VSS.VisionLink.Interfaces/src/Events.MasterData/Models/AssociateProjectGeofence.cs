using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
{
  public class AssociateProjectGeofence : IProjectEvent
  {
    public string ProjectUID { get; set; }
    public string GeofenceUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}

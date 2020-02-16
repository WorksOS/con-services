using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class DeleteGeofenceEvent:IGeofenceEvent
  {
    public Guid GeofenceUID { get; set; }
    public Guid? UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}

using System;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
  interface IGeofenceEvent
  {
    Guid GeofenceUID { get; set; }
    Guid? UserUID { get; set; }
    DateTime ActionUTC { get; set; }
  }
}

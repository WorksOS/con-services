using System;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
  public interface IDeviceEvent
  {
    DateTime ActionUTC { get; set; }
  }
}

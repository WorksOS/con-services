using System;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
  public interface IGroupEvent
  {
    Guid GroupUID { get; set; }
    DateTime ActionUTC { get; set; }
  }
}
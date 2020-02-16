using System;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
  public interface IWorkDefinitionEvent
  {
    DateTime ActionUTC { get; set; }
  }
}

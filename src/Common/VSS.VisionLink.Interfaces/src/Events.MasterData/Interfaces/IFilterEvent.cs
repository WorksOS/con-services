using System;

namespace VSS.Visionlink.Interfaces.Events.MasterData.Interfaces
{
  public interface IFilterEvent
  {
    Guid FilterUID { get; set; }
    DateTime ActionUTC { get; set; }
  }
}

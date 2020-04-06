using System;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces
{
  public interface IFilterEvent
  {
    Guid FilterUID { get; set; }
    DateTime ActionUTC { get; set; }
  }
}

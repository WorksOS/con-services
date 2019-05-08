using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Interfaces
{
  public interface IFilterEvent
  {
    Guid FilterUID { get; set; }
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}
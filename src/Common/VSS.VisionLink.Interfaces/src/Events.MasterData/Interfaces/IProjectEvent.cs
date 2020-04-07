using System;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces
{
  public interface IProjectEvent
  {
    Guid ProjectUID { get; set; }
    DateTime ActionUTC { get; set; }
  }
}

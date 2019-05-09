using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Interfaces
{
  public interface IWorkDefinitionEvent
  {
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}
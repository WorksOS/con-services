using System;

namespace VSS.TRex.Events.Interfaces
{
  public interface IProductionEventsFactory
  {
    IProductionEvents NewEventList(short machineID, Guid siteModelID, ProductionEventType eventType);
  }
}

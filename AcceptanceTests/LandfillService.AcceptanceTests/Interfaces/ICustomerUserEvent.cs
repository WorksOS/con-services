using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Interfaces
{
  public interface ICustomerUserEvent : ICustomerEvent
  {
    Guid UserUID { get; set; }
  }
}
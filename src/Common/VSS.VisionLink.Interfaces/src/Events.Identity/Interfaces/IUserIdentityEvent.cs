using System;

namespace VSS.VisionLink.Interfaces.Events.Identity.Interfaces
{
  public interface IUserIdentityEvent
  {
    string UserUID { get; set; }
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}

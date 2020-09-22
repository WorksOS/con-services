using System;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignChangedEventSenderResponse
  {
    bool Success { get; set; }

    Guid NodeUid { get; set; }
  }
}

using System;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
  public interface ISiteModelAttributesChangedEventSenderResponse
  {
    bool Success { get; set; }

    Guid NodeUid { get; set; }
  }
}

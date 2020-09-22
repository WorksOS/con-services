using System;

namespace VSS.TRex.SiteModels.Interfaces.Listeners
{
  public interface IRebuildSiteModelTAGNotifierEventSenderResponse
  {
    public bool Success { get; set; }
    public Guid NodeUid { get; set; }
  }
}

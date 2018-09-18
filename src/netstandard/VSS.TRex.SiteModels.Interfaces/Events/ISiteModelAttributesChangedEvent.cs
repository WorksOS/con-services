using System;

namespace VSS.TRex.SiteModels.Interfaces.Events
{
  public interface ISiteModelAttributesChangedEvent
  {
    Guid SiteModelID { get; set; }
  }
}

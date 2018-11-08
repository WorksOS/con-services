using System;

namespace VSS.TRex.SiteModels.Interfaces.Events
{
  public interface ISiteModelAttributesChangedEventListener
  {
    void StartListening();
    void StopListening();
  }
}

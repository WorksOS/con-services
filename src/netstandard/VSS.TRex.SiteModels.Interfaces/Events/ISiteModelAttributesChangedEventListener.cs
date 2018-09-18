using System;

namespace VSS.TRex.SiteModels.Interfaces.Events
{
  public interface ISiteModelAttributesChangedEventListener
  {
    bool Invoke(Guid nodeId, ISiteModelAttributesChangedEvent message);
    void StartListening();
    void StopListening();
  }
}

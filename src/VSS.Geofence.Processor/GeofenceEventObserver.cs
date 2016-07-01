using VSS.Geofence.Data.Interfaces;
using VSS.MasterData.Common.JsonConverters;
using VSS.MasterData.Common.Processor;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Geofence.Processor
{
  public class GeofenceEventObserver : EventObserverBase<IGeofenceEvent, GeofenceEventConverter>
  {
    private IGeofenceService _geofenceService;

    public GeofenceEventObserver(IGeofenceService geofenceService)
    {
      _geofenceService = geofenceService;
      EventName = "Geofence";
    }

    protected override bool ProcessEvent(IGeofenceEvent evt)
    {
      int updatedCount = _geofenceService.StoreGeofence(evt);
      return updatedCount == 1;    
    }

  }
}

using VSS.Geofence.Data.Interfaces;
using VSS.Landfill.Common.JsonConverters;
using VSS.Landfill.Common.Processor;
using VSS.Subscription.Data.Interfaces;
using VSS.Project.Data.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Subscription.Processor
{
  public class SubscriptionEventObserver : EventObserverBase<ISubscriptionEvent, SubscriptionEventConverter>
    {
        private ISubscriptionService _subscriptionService;
        private IProjectService _projectService;
        private IGeofenceService _geofenceService;

        public SubscriptionEventObserver(ISubscriptionService subscriptionService, IProjectService projectService, IGeofenceService geofenceService)
        {
            _subscriptionService = subscriptionService;
            _projectService = projectService;
            _geofenceService = geofenceService;
            EventName = "Subscription";
        }

        protected override bool ProcessEvent(ISubscriptionEvent evt)
        {
          int updatedCount = _subscriptionService.StoreSubscription(evt, _projectService);
          return updatedCount == 1;
        }
    }
}

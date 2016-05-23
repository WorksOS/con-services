using System;
using System.Reflection;
using log4net;
using VSS.Geofence.Data.Interfaces;
using VSS.Landfill.Common.JsonConverters;
using VSS.Landfill.Common.Processor;
using VSS.Subscription.Data.Interfaces;
using VSS.Project.Data.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Subscription.Processor
{
  public class SubscriptionEventObserver : EventObserverBase<ISubscriptionEvent, SubscriptionEventConverter>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ISubscriptionService _subscriptionService;
        private IProjectService _projectService;

        public SubscriptionEventObserver(ISubscriptionService subscriptionService, IProjectService projectService)
        {
            _subscriptionService = subscriptionService;
            _projectService = projectService;
            EventName = "Subscription";
        }

        protected override bool ProcessEvent(ISubscriptionEvent evt)
        {
          int updatedCount = 0;
          if (evt is AssociateProjectSubscriptionEvent)
          {
            //Handle out of order events. Create a dummy project if required.
            var subscriptionEvent = (AssociateProjectSubscriptionEvent)evt;
            var lastActionUtc = subscriptionEvent.ActionUTC;
            var project = _projectService.GetProject(subscriptionEvent.ProjectUID.ToString());
            if (project == null)
            {
              lastActionUtc = DateTime.MinValue;
              updatedCount = _projectService.StoreProject(
                    new CreateProjectEvent
                    {
                      ProjectUID = subscriptionEvent.ProjectUID,
                      ProjectName = string.Empty,
                      ProjectTimezone = string.Empty,
                      ActionUTC = lastActionUtc
                    });
              if (updatedCount == 0)
              {
                Log.WarnFormat("SubscriptionEventObserver: Failed to create dummy project for out of order event - subscription UID {0}, project UID {1}", 
                subscriptionEvent.SubscriptionUID, subscriptionEvent.ProjectUID);                
              }
            }
            updatedCount = _projectService.AssociateProjectSubscription(subscriptionEvent.ProjectUID.ToString(),
                subscriptionEvent.SubscriptionUID.ToString(), lastActionUtc);
            if (updatedCount == 0)
            {
              Log.WarnFormat("SubscriptionEventObserver: Failed to save subscription UID {0} for project UID {1}", 
                subscriptionEvent.SubscriptionUID, subscriptionEvent.ProjectUID);
              
            }
          }
          updatedCount = _subscriptionService.StoreSubscription(evt);

          return updatedCount == 1;
        }
    }
}

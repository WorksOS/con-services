using System;
using System.Reflection;
using log4net;
using VSS.Geofence.Data.Interfaces;
using VSS.MasterData.Common.JsonConverters;
using VSS.MasterData.Common.Processor;
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

        public SubscriptionEventObserver(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
            EventName = "Subscription";
        }

        protected override bool ProcessEvent(ISubscriptionEvent evt)
        {
          int updatedCount = _subscriptionService.StoreSubscription(evt);
          return updatedCount == 1;
        }
    }
}

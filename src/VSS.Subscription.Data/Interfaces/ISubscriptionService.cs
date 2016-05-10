using System;
using System.Collections.Generic;
using VSS.Project.Data.Interfaces;
using VSS.Subscription.Data.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Subscription.Data.Interfaces
{
	public interface ISubscriptionService
	{
    int StoreSubscription(ISubscriptionEvent evt, IProjectService projectService);
   
	}
}

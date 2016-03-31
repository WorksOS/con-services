using System;
using System.Collections.Generic;
using VSS.Subscription.Data.Models;

namespace VSS.Subscription.Data.Interfaces
{
	public interface ISubscriptionService
	{
        int StoreSubscription(ISubscriptionEvent evt);
        List<CustomerSubscriptionModel> GetSubscriptionForCustomer(Guid customerGuid);
        List<ActiveProjectCustomerSubscriptionModel> GetActiveProjectSubscriptionForCustomer(Guid customerGuid);
	      int GetProjectBySubscription(string projectSubscriptionUid);
	}
}

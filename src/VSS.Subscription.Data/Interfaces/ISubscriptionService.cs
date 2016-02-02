using System;
using System.Collections.Generic;
using VSS.Subscription.Data.Models;

namespace VSS.Subscription.Model.Interfaces
{
	public interface ISubscriptionService
	{
        void CreateAssetSubscription(CreateAssetSubscriptionEvent assetSubscription);
        void UpdateAssetSubscription(UpdateAssetSubscriptionEvent updateSubscription);
        void CreateProjectSubscription(CreateProjectSubscriptionEvent createSubscription);
        void UpdateProjectSubscription(UpdateProjectSubscriptionEvent updateSubscription);
        void AssociateProjectSubscription(AssociateProjectSubscriptionEvent associateSubscription);
        void DissociateProjectSubscription(DissociateProjectSubscriptionEvent dissociateSubscription);
        void CreateCustomerSubscription(CreateCustomerSubscriptionEvent createSubscription);
        void UpdateCustomerSubscription(UpdateCustomerSubscriptionEvent updateSubscription);
        List<CustomerSubscriptionModel> GetSubscriptionForCustomer(Guid customerGuid);
        List<CustomerSubscriptionModel> GetActiveProjectSubscriptionForCustomer(Guid customerGuid);
	      int GetProjectBySubscripion(string projectSubscriptionUid);
	}
}

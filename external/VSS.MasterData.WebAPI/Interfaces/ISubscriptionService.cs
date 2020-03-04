using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;

namespace VSS.MasterData.WebAPI.Interfaces
{
	public interface ISubscriptionService
	{
		bool CreateAssetSubscriptions(List<CreateAssetSubscriptionEvent> createSubscriptionList);

		bool UpdateAssetSubscriptions(List<UpdateAssetSubscriptionEvent> updateSubscriptionList);

		bool CreateAssetSubscription(CreateAssetSubscriptionEvent createSubscription);

		bool UpdateAssetSubscription(UpdateAssetSubscriptionEvent updateSubscription);

		bool CreateCustomerSubscription(CreateCustomerSubscriptionEvent createSubscription);

		bool UpdateCustomerSubscription(UpdateCustomerSubscriptionEvent updateSubscription);

		bool CreateProjectSubscription(CreateProjectSubscriptionEvent createSubscription);

		bool UpdateProjectSubscription(UpdateProjectSubscriptionEvent updateSubscription);

		bool AssociateProjectSubscription(AssociateProjectSubscriptionEvent associateSubscription);

		bool DissociateProjectSubscription(DissociateProjectSubscriptionEvent dissociateSubscription);

		AssetSubscriptionModel GetSubscriptionForAsset(Guid assetGuid);

		IEnumerable<CustomerSubscriptionModel> GetSubscriptionForCustomer(Guid customerGuid);

		IEnumerable<ActiveProjectCustomerSubscriptionModel> GetActiveProjectSubscriptionForCustomer(Guid customerGuid);

		bool CheckExistingSubscription(Guid subscriptionGuid, string createSubscriptionType);

		DbAssetSubscription GetExistingAssetSubscription(Guid subscriptionGuid);

		DbCustomerSubscription GetExistingCustomerSubscription(Guid subscriptionGuid);

		DbProjectSubscription GetExistingProjectSubscription(Guid subscriptionGuid);
	}
}
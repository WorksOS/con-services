
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon.Services.MDM.Interfaces
{
    public interface ISubscriptionService
	{
		    bool CreateAssetSubscription(object assetSubscription);
        bool UpdateAssetSubscription(object assetSubscription);
        bool CreateProjectSubscription(object projectSubscription);
        bool UpdateProjectSubscription(object projectSubscription);
        bool CreateCustomerSubscription(object customerSubscription);
        bool UpdateCustomerSubscription(object customerSubscription);
        bool AssociateProjectSubscription(object association);
	}
}

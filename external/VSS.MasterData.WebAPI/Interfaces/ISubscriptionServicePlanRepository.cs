using DbModel.DeviceConfig;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
	public interface ISubscriptionServicePlanRepository
	{
		Task<IEnumerable<SubscriptionServicePlanDto>> FetchSubscription(string assetUid, string endDate);
		Task<IDictionary<int, List<SubscriptionServicePlanDto>>> FetchSubscription(IEnumerable<string> assetUids, string endDate);
	}
}

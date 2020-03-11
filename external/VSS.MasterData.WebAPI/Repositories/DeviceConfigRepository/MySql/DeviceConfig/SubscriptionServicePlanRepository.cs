using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;
using Interfaces;
using DbModel;
using DbModel.DeviceConfig;
using VSS.MasterData.WebAPI.Transactions;

namespace DeviceConfigRepository.MySql.DeviceConfig
{
	public class SubscriptionServicePlanRepository : ISubscriptionServicePlanRepository
	{
		private ILoggingService _loggingService;
		private ITransactions _transactions;

		public SubscriptionServicePlanRepository(ITransactions transactions, ILoggingService loggingService)
		{
			_transactions = transactions;
			_loggingService = loggingService;
			this._loggingService.CreateLogger(this.GetType());
		}

		public async Task<IEnumerable<SubscriptionServicePlanDto>> FetchSubscription(string assetUid, string endDate)
		{
			try
			{
				return await this._transactions.GetAsync<SubscriptionServicePlanDto>(Queries.FETCH_SUBSCRIPTION_SERVICETYPE, new
				{
					assetUids = assetUid.Replace("-", string.Empty),
					endDate = endDate
				});
			}
			catch (Exception ex)
			{
				this._loggingService.Error("An Unhandled Exception has occurred", "DeviceTypeRepository.FetchDeviceTypes", ex);
				throw;
			}
		}

		public async Task<IDictionary<int, List<SubscriptionServicePlanDto>>> FetchSubscription(IEnumerable<string> assetUids, string endDate)
		{
			try
			{
				assetUids = assetUids.Select(x => "unhex('" + x.Replace("-",string.Empty) + "')");
				var subscriptions = await this._transactions.GetAsync<SubscriptionServicePlanDto>(string.Format(Queries.FETCH_SUBSCRIPTION_SERVICETYPE, string.Join(",", assetUids)), new
				{
					endDate = endDate
				});

				return subscriptions.GroupBy(x => x.ServiceTypeID).ToDictionary(x => x.Key, x => x.ToList());
			}
			catch (Exception ex)
			{
				this._loggingService.Error("An Unhandled Exception has occurred", "DeviceTypeRepository.FetchDeviceTypes", ex);
				throw;
			}
		}
	}
}

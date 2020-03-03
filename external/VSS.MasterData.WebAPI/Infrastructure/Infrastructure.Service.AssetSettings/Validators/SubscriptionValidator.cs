using Interfaces;
using ClientModel.AssetSettings.Request;
using CommonModel.Error;
using Infrastructure.Cache.Interfaces;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.AssetSettings.Validators
{
	public class SubscriptionValidator : RequestValidatorBase, IRequestValidator<AssetSettingValidationRequestBase>
	{
		private readonly ISubscriptionServicePlanRepository _subscriptionServicePlanRepository;
		private readonly IServiceTypeParameterCache _serviceTypeParameterCache;
		private readonly IParameterAttributeCache _parameterAttributeCache;
		private readonly ILoggingService _loggingService;

		public SubscriptionValidator(ISubscriptionServicePlanRepository subscriptionServicePlanRepository, IServiceTypeParameterCache serviceTypeParameterCache, IParameterAttributeCache parameterAttributeCache, ILoggingService loggingService) : base(loggingService)
		{
			_subscriptionServicePlanRepository = subscriptionServicePlanRepository;
			_serviceTypeParameterCache = serviceTypeParameterCache;
			_parameterAttributeCache = parameterAttributeCache;
		}

		public async Task<IList<IErrorInfo>> Validate(AssetSettingValidationRequestBase request)
		{
			var result = new List<IErrorInfo>();
			var deviceTypeParameterAttributes = await this._parameterAttributeCache.Get(request.DeviceType, request.GroupName);
			if (deviceTypeParameterAttributes != null && deviceTypeParameterAttributes.Any(x => x.IncludeInd))
			{
				var subscriptionPlans = await _subscriptionServicePlanRepository.FetchSubscription(request.AssetUIDs, DateTime.Now.ToDateTimeStringWithYearMonthDayFormat());
				foreach (var parameter in deviceTypeParameterAttributes.Where(x => x.IncludeInd).GroupBy(x => x.ParameterName))
				{
					var serviceTypeParameters = await this._serviceTypeParameterCache.Get(parameter.Key);
					foreach (var serviceTypeParameter in serviceTypeParameters)
					{
						if (subscriptionPlans.ContainsKey(serviceTypeParameter.ServiceTypeID))
						{
							if (serviceTypeParameter.IncludeInd)
							{
								var subscriptionNotAvailableAssets = request.AssetUIDs.Except(subscriptionPlans[serviceTypeParameter.ServiceTypeID].Select(x => x.AssetUID.ToString()));
								if (subscriptionNotAvailableAssets.Any())
								{
									result.AddRange(base.GetValidationResults(ErrorCodes.AssetSubscriptionIsInvalid, subscriptionNotAvailableAssets, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.AssetSubscriptionIsInvalid), serviceTypeParameter.DeviceParamGroupName + "\\" + serviceTypeParameter.DeviceParameterName), true, "SubscriptionValidator.Validate"));
									request.AssetUIDs.RemoveAll(subscriptionNotAvailableAssets.Contains);
								}
							}
							else
							{
								var subscriptionShouldNotBeAvailableAssets = request.AssetUIDs.Intersect(subscriptionPlans[serviceTypeParameter.ServiceTypeID].Select(x => x.AssetUID.ToString()));
								if (subscriptionShouldNotBeAvailableAssets.Any())
								{
									result.AddRange(base.GetValidationResults(ErrorCodes.AssetSubscriptionIsInvalid, subscriptionShouldNotBeAvailableAssets, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.AssetSubscriptionIsInvalid), serviceTypeParameter.DeviceParamGroupName + "\\" + serviceTypeParameter.DeviceParameterName), true, "SubscriptionValidator.Validate"));
									request.AssetUIDs.RemoveAll(subscriptionShouldNotBeAvailableAssets.Contains);
								}

							}
						}
					}
				}
			}
			return result;
		}
	}
}

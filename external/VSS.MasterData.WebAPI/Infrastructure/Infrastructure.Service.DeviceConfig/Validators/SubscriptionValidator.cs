using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.Error;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.DeviceConfig.Validators
{
	public class SubscriptionValidator : RequestValidatorBase, IRequestValidator<DeviceConfigRequestBase>
	{
		private readonly ISubscriptionServicePlanRepository _subscriptionServicePlanRepository;
		private readonly IServiceTypeParameterCache _serviceTypeParameterCache;
		private readonly IParameterAttributeCache _parameterAttributeCache;

		public SubscriptionValidator(ISubscriptionServicePlanRepository subscriptionServicePlanRepository, IServiceTypeParameterCache serviceTypeParameterCache, IParameterAttributeCache parameterAttributeCache, ILoggingService loggingService) : base(loggingService)
		{
			_subscriptionServicePlanRepository = subscriptionServicePlanRepository;
			_serviceTypeParameterCache = serviceTypeParameterCache;
			_parameterAttributeCache = parameterAttributeCache;
		}

		public async Task<IList<IErrorInfo>> Validate(DeviceConfigRequestBase request)
		{
			var result = new List<IErrorInfo>();

			if (request.AssetUIDs != null && request.AssetUIDs.Any())
			{
				var requestedAssetUIDs = request.AssetUIDs.Select(x => Guid.Parse(x).ToString()).ToList();
				var deviceTypeParameterAttributes = await this._parameterAttributeCache.Get(request.DeviceType, request.ParameterGroupName);
				if (deviceTypeParameterAttributes != null && deviceTypeParameterAttributes.Any(x => x.IncludeInd))
				{
					var subscriptionPlans = await _subscriptionServicePlanRepository.FetchSubscription(requestedAssetUIDs, DateTime.Now.ToDateTimeStringWithYearMonthDayFormat());
					foreach (var parameter in deviceTypeParameterAttributes.Where(x => x.IncludeInd).GroupBy(x => x.ParameterName))
					{
						var serviceTypeParameters = await this._serviceTypeParameterCache.Get(parameter.Key);
						foreach (var serviceTypeParameter in serviceTypeParameters)
						{
							if (subscriptionPlans.ContainsKey(serviceTypeParameter.ServiceTypeID))
							{
								if (serviceTypeParameter.IncludeInd)
								{
									var subscriptionNotAvailableAssets = requestedAssetUIDs.Except(subscriptionPlans[serviceTypeParameter.ServiceTypeID].Select(x => x.AssetUID.ToString()));
									if (subscriptionNotAvailableAssets.Any())
									{
										result.AddRange(base.GetValidationResults(ErrorCodes.AssetSubscriptionIsInvalid, subscriptionNotAvailableAssets, string.Format(Utils.GetEnumDescription(ErrorCodes.AssetSubscriptionIsInvalid), serviceTypeParameter.DeviceParamGroupName + "/" + serviceTypeParameter.DeviceParameterName), false, "SubscriptionValidator.Validate"));
									}
								}
								else
								{
									var subscriptionShouldNotBeAvailableAssets = requestedAssetUIDs.Intersect(subscriptionPlans[serviceTypeParameter.ServiceTypeID].Select(x => x.AssetUID.ToString()));
									if (subscriptionShouldNotBeAvailableAssets.Any())
									{
										result.AddRange(base.GetValidationResults(ErrorCodes.AssetSubscriptionIsInvalid, subscriptionShouldNotBeAvailableAssets, string.Format(Utils.GetEnumDescription(ErrorCodes.AssetSubscriptionIsInvalid), serviceTypeParameter.DeviceParamGroupName + "/" + serviceTypeParameter.DeviceParameterName), false, "SubscriptionValidator.Validate"));
									}
								}
							}
						}
					}
					var parameterCount = deviceTypeParameterAttributes.Select(x => x.ParameterName).Distinct().Count();
					var assetErrorLists = result.Select(x => x as AssetErrorInfo).ToList();
					var assetErrorListsGrouped = result.Select(x => x as AssetErrorInfo).ToList().GroupBy(x => x.AssetUID);
					foreach (var asset in assetErrorListsGrouped)
					{
						if (parameterCount == asset.ToList().Count())
						{
							assetErrorLists.RemoveAll(x => x.AssetUID == asset.Key);
							assetErrorLists.Add(base.GetValidationResult(ErrorCodes.AssetSubscriptionIsInvalid, asset.Key, string.Format(Utils.GetEnumDescription(ErrorCodes.AssetSubscriptionIsInvalid), request.ParameterGroupName), false, "SubscriptionValidator.Validate") as AssetErrorInfo);
						}
					}
					result = assetErrorLists.Select(x => x as IErrorInfo).ToList();
				}
				request.AssetUIDs = requestedAssetUIDs;
			}
			return result;
		}
	}
}

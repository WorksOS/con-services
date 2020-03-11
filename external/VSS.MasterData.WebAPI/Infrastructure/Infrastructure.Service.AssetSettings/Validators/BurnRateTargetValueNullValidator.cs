using Interfaces;
using ClientModel.AssetSettings.Request.AssetSettings;
using CommonModel.Error;
using DbModel.AssetSettings;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.AssetSettings.Validators
{
	public class BurnRateTargetValueNullValidator : RequestValidatorBase, IRequestValidator<AssetFuelBurnRateSettingRequest>
    {
        private readonly IAssetConfigRepository _assetSettingsRepository;
        public BurnRateTargetValueNullValidator(IAssetConfigRepository assetSettingsRepository, ILoggingService loggingService) : base(loggingService)
        {
            _assetSettingsRepository = assetSettingsRepository;
        }

        public async Task<IList<IErrorInfo>> Validate(AssetFuelBurnRateSettingRequest request)
        {

            var assetsSettingsResponse = await _assetSettingsRepository.FetchAssetConfig(request.AssetUIds, new AssetSettingsDto
            {
                StartDate = request.StartDate,
                TargetValues = AssignAssetTargetValues(request.TargetValues, new AssetSettingsDto()),
                FilterCriteria = new List<KeyValuePair<string, Tuple<string, object>>>
                {
                    new KeyValuePair<string, Tuple<string, object>>("<=", new Tuple<string, object>("AC.StartDate", request.StartDate.ToDateTimeStringWithYearMonthDayFormat())),
                    new KeyValuePair<string, Tuple<string, object>>("is", new Tuple<string, object>("AC.EndDate", null))
                }
            });

            if (assetsSettingsResponse?.Count() == 0)
            {
                if (!(request.WorkTargetValue.HasValue && request.IdleTargetValue.HasValue))
                {
                    return new List<IErrorInfo> { GetValidationResult(ErrorCodes.WorkOrIdleValuesShouldNotBeNull, UtilHelpers.GetEnumDescription(ErrorCodes.WorkOrIdleValuesShouldNotBeNull), true, MethodBase.GetCurrentMethod().Name) };
                }
            }
            else if (request.WorkTargetValue.HasValue != request.IdleTargetValue.HasValue)
            {
                return new List<IErrorInfo> { GetValidationResult(ErrorCodes.BothWorkAndIdleValuesShouldBeNullORShouldNotBeNull, UtilHelpers.GetEnumDescription(ErrorCodes.BothWorkAndIdleValuesShouldBeNullORShouldNotBeNull), true, MethodInfo.GetCurrentMethod().Name) };
            }
            return null;
        }
        private IDictionary<AssetTargetType, Tuple<Guid, double>> AssignAssetTargetValues(IDictionary<AssetTargetType, double?> requestTargetValues, AssetSettingsDto asset)
        {
            IDictionary<AssetTargetType, Tuple<Guid, double>> targetValues = new Dictionary<AssetTargetType, Tuple<Guid, double>>();
            if (asset.TargetValues == null)
            {
                asset.TargetValues = new Dictionary<AssetTargetType, Tuple<Guid, double>>();
            }
            foreach (var targetValue in requestTargetValues)
            {
                Tuple<Guid, double> tempTargetValue = new Tuple<Guid, double>(Guid.Empty, 0);
                if (asset.TargetValues.ContainsKey(targetValue.Key))
                {
                    tempTargetValue = new Tuple<Guid, double>(asset.TargetValues[targetValue.Key].Item1, Convert.ToDouble(targetValue.Value));
                }
                targetValues.Add(targetValue.Key, tempTargetValue);
            }
            return targetValues;
        }
    }
}

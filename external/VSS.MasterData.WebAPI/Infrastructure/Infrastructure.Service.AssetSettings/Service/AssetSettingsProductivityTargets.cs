using Interfaces;
using CommonModel.AssetSettings;
using CommonModel.Error;
using CommonModel.Exceptions;
using DbModel.AssetSettings;
using Infrastructure.Service.AssetSettings.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace Infrastructure.Service.AssetSettings.Service
{
	public class AssetSettingsProductivityTargets : AssetWeeklySettingsService
    {
        public AssetSettingsProductivityTargets(IWeeklyAssetSettingsRepository targetsRepo, IAssetSettingsTypeHandler<AssetSettingsBase> handler, AssetSettingsOverlapTemplate assetSettingsOverlap, IValidationHelper validationHelper, ITransactions transactions, ILoggingService loggingService, IAssetSettingsPublisher assetSettingsPublisher): base(transactions, loggingService)
        {
            _weekRepo = targetsRepo;
            _Converter = handler;
            _assetSettingsOverlap = assetSettingsOverlap;
            _validationHelper = validationHelper;
            _groupType = Enums.GroupType.ProductivityTargets;
			_assetSettingsPublisher = assetSettingsPublisher;
            _loggingService = loggingService;
            _loggingService.CreateLogger(typeof(AssetSettingsProductivityTargets));

        }
        protected override List<AssetErrorInfo> DoValidation(AssetSettingsBase[] assetSettings)
        {
            try
            {
                var isValidAssetUID = _validationHelper.ValidateAssetUIDParameters(assetSettings.Select(t => t.AssetUID.ToString()).ToArray());
                var isValidDateParameters = _validationHelper.validateStartDateAndEndDate(assetSettings);
                var isNegatedValuesThere = _validationHelper.ValidateProductivityTargetsForNegativeValues(assetSettings as ProductivityWeeklyTargetValues[]);
                //var inValidAssetUID = isValidDateParameters.Select(dateParam => dateParam.AssetUID).Union(isValidTargetRuntime.Select(runtime => runtime.AssetUID)).Union(isValidRuntimeAndIdleHours.Select(runtime => runtime.AssetUID)).ToList().Distinct();
                var isDateValid = _validationHelper.ValidateDatetimeInAssetSettings(assetSettings);
                var isValid = isValidAssetUID.Union(isValidDateParameters).Union(isNegatedValuesThere).Union(isDateValid).Distinct().ToList();
                _loggingService.Debug(JsonConvert.SerializeObject(isValid), "AssetSettingsProductivityTargets.DoValidation");
                return isValid;

            }
            catch (DomainException ex)
            {
                return new List<AssetErrorInfo>();
            }
        }

        protected override List<AssetErrorInfo> DoValidation(string[] AssetUID, DateTime? startDate, DateTime? endDate)
        {
            //_loggingService.Debug("", "AssetSettingsTemplate.DoValidation");
            var isValidAssetUID = _validationHelper.ValidateAssetUIDParameters(AssetUID);
            var isValidDateParameters = new List<AssetErrorInfo>();
            if (startDate.HasValue && endDate.HasValue)
                isValidDateParameters = _validationHelper.validateStartDateAndEndDate(startDate.Value, endDate.Value);
            var isValidAssetUIDParam = _validationHelper.ValidateAssetUIDsForDefaultGuid(AssetUID.Where(id => !isValidAssetUID.Select(Id => Id.AssetUID).Contains(id)).ToArray());
            var isValid = isValidAssetUID.Union(isValidDateParameters).Union(isValidAssetUIDParam).Distinct().ToList();
            _loggingService.Debug(JsonConvert.SerializeObject(isValid), "AssetSettingsProductivityTargets.DoValidation");
            return isValid;
        }

        protected override List<AssetSettingsGetDBResponse> GetAssetSettingsForTargetTypeWithAssetUID(string assetUIDs)
        {
            return _weekRepo.GetProductivityTargetsDetailsByAssetId(assetUIDs);
        }

        protected override List<AssetSettingsGetDBResponse> GetAssetSettingsForTargetTypeWithAssetUIDStartDateAndEndDate(string assestUIDs, DateTime startDate, DateTime endDate)
        {
            return _weekRepo.GetProductivityTargetsByStartDateAndAssetUID(assestUIDs, startDate, endDate);
        }
    }
}

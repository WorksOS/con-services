using Interfaces;
using CommonModel.AssetSettings;
using CommonModel.Error;
using CommonModel.Exceptions;
using DbModel.AssetSettings;
using Infrastructure.Service.AssetSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace Infrastructure.Service.AssetSettings.Service
{
	public class AssetSettingsTargets : AssetWeeklySettingsService
    {
        public AssetSettingsTargets(ITransactions transactions, IWeeklyAssetSettingsRepository targetsRepo, IAssetSettingsTypeHandler<AssetSettingsBase> handler, AssetSettingsOverlapTemplate assetSettingsOverlap, IValidationHelper validationHelper, ILoggingService loggingService, IAssetSettingsPublisher assetSettingsPublisher) : base(transactions, loggingService)
        {
            _weekRepo = targetsRepo;
            _Converter = handler;
            _assetSettingsOverlap = assetSettingsOverlap;
            _validationHelper = validationHelper;
            _groupType = Enums.GroupType.AssetTargets;
			_assetSettingsPublisher = assetSettingsPublisher;
            _loggingService = loggingService;
            _loggingService.CreateLogger(typeof(AssetSettingsTargets));
        }

        protected override List<AssetErrorInfo> DoValidation(AssetSettingsBase[] assetSettings)
        {
            try
            {
                var isValidAssetUID = _validationHelper.ValidateAssetUIDParameters(assetSettings.Select(t => t.AssetUID.ToString()).ToArray());
                var isValidDateParameters = _validationHelper.validateStartDateAndEndDate(assetSettings);
                var isValidTargetRuntime = _validationHelper.ValidateAssetTargetHours(assetSettings as AssetSettingsWeeklyTargets[]);
                var isValidRuntimeAndIdleHours = _validationHelper.ValidateTargetRuntimeWithIdleHours(assetSettings as AssetSettingsWeeklyTargets[]);
                var isDateValid = _validationHelper.ValidateDatetimeInAssetSettings(assetSettings);
                //var inValidAssetUID = isValidDateParameters.Select(dateParam => dateParam.AssetUID).Union(isValidTargetRuntime.Select(runtime => runtime.AssetUID)).Union(isValidRuntimeAndIdleHours.Select(runtime => runtime.AssetUID)).ToList().Distinct();
                return isValidAssetUID.Union(isValidDateParameters).Union(isValidTargetRuntime).Union(isValidRuntimeAndIdleHours).Union(isDateValid).Distinct().ToList();
            }
            catch (DomainException ex)
            {
                return new List<AssetErrorInfo>();
            }
        }

        protected override List<AssetErrorInfo> DoValidation(string[] AssetUID, DateTime? startDate, DateTime? endDate)
        {
            _loggingService.Debug("", "AssetSettingsTemplate.DoValidation");
            var isValidAssetUID = _validationHelper.ValidateAssetUIDParameters(AssetUID);
            var isValidDateParameters = new List<AssetErrorInfo>();
            if (startDate.HasValue && endDate.HasValue)
                isValidDateParameters = _validationHelper.validateStartDateAndEndDate(startDate.Value, endDate.Value);
            var isValidAssetUIDParam = _validationHelper.ValidateAssetUIDsForDefaultGuid(AssetUID.Where(id => !isValidAssetUID.Select(Id => Id.AssetUID).Contains(id)).ToArray());
            return isValidAssetUID.Union(isValidDateParameters).Union(isValidAssetUIDParam).Distinct().ToList();
        }

        protected override List<AssetSettingsGetDBResponse> GetAssetSettingsForTargetTypeWithAssetUID(string assetUIDs)
        {
            return _weekRepo.GetAssetUtilizationTargetRunTimeByAssetUID(assetUIDs);
        }

        protected override List<AssetSettingsGetDBResponse> GetAssetSettingsForTargetTypeWithAssetUIDStartDateAndEndDate(string assestUIDs, DateTime startDate, DateTime endDate)
        {
            return _weekRepo.GetAssetUtilizationTargetRunTimeByStartDateAndAssetUID(assestUIDs, startDate, endDate);
        }
    }
}

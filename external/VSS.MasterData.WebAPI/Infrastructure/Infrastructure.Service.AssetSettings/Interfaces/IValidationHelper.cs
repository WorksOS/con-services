using CommonModel.AssetSettings;
using CommonModel.Error;
using System;
using System.Collections.Generic;

namespace Infrastructure.Service.AssetSettings.Interfaces
{
	public interface IValidationHelper
    {
        List<AssetErrorInfo> ValidateAssetUIDParameters(string[] assetUID);
        List<AssetErrorInfo> ValidateAssetTargetHours(AssetSettingsWeeklyTargets[] assetTargets);
        List<AssetErrorInfo> ValidateAssetUIDsForDefaultGuid(string[] assetUID);
        List<AssetErrorInfo> ValidateTargetRuntimeWithIdleHours(AssetSettingsWeeklyTargets[] assetTargets);
        List<AssetErrorInfo> validateStartDateAndEndDate(AssetSettingsBase[] assetTargets);
        List<AssetErrorInfo> validateStartDateAndEndDate(DateTime startDate, DateTime endDate);
        List<AssetErrorInfo> ValidateProductivityTargetsForNegativeValues(ProductivityWeeklyTargetValues[] values);
        List<AssetErrorInfo> ValidateDatetimeInAssetSettings(AssetSettingsBase[] values);
    }
}

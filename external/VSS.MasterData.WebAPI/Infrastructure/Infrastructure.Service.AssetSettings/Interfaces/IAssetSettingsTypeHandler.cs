using CommonModel.AssetSettings;
using DbModel.AssetSettings;
using Infrastructure.Service.AssetSettings.Enums;
using System;
using System.Collections.Generic;

namespace Infrastructure.Service.AssetSettings.Interfaces
{
	public interface IAssetSettingsTypeHandler<T> where T:AssetSettingsBase
    {
        List<AssetSettingsGetDBResponse> UpdateDateForEndDateAndReturn(IEnumerable<AssetSettingsGetDBResponse> assetSettings, DateTime dateTime, GroupType targetType);
        List<AssetSettingsGetDBResponse> UpdateDataForStartDate(IEnumerable<AssetSettingsGetDBResponse> assetSettings, DateTime dateTime, GroupType targetType);
        List<AssetSettingsGetDBResponse> GetCommonResponseFromProductivityTargetsAndAssetTargets(T targets);
        List<T> ExtractToAssetUtilizationTargets(List<AssetSettingsGetDBResponse> assetTargets, GroupType groupType);
        Tuple<DateTime, DateTime> GetStartDateAndEndDate(List<AssetSettingsGetDBResponse> assetTargets, GroupType groupType);
    }
}

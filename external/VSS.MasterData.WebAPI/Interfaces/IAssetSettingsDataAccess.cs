using DbModel.AssetSettings;
using System;
using System.Collections.Generic;

namespace Interfaces
{
	public interface IAssetSettingsDataAccess
    {
        List<AssetSettingsGetDBResponse> GetAssetUtilizationTargetRunTimeByStartDateAndAssetUID(string AssetUIDs, DateTime startDate, DateTime endDate);
        List<AssetSettingsGetDBResponse> GetAssetUtilizationTargetRunTimeByAssetUID(string AssetUIDs);
        int InsertAssetTargets(List<AssetSettingsGetDBResponse> targetDBResponse);
        int UpdateAssetTargets(List<AssetSettingsGetDBResponse> targetDBResponse);
        int DeleteAssetTargets(string assetWeeklyIdentifiers);
    }
}

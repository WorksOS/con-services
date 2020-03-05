using DbModel.AssetSettings;
using System;
using System.Collections.Generic;

namespace Interfaces
{
	public interface IWeeklyAssetSettingsRepository
    {
        List<AssetSettingsGetDBResponse> GetAssetUtilizationTargetRunTimeByStartDateAndAssetUID(string AssetUIDs, DateTime startDate, DateTime endDate);
        List<AssetSettingsGetDBResponse> GetAssetUtilizationTargetRunTimeByAssetUID(string AssetUIDs);        
        List<AssetSettingsGetDBResponse> GetProductivityTargetsByStartDateAndAssetUID(string AssetUIDs, DateTime startDate, DateTime endDate);
        List<AssetSettingsGetDBResponse> GetProductivityTargetsDetailsByAssetId(string AssetUID);
    }
}

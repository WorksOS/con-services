using ClientModel.AssetSettings.Request;
using DbModel.AssetSettings;
using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Transactions;

namespace Infrastructure.Service.AssetSettings.Interfaces
{
	public interface IAssetSettingsPublisher
    {
        bool publishAssetWeeklySettings(List<AssetWeeklyTargetsDto> targets);
        bool PublishAssetSettings(IEnumerable<AssetSettingsDto> settingsDto);
        void PublishUserAssetSettings(AssetSettingsRequestBase request);
        void PublishUserWeeklyAssetSettings(List<AssetSettingsGetDBResponse> request, Guid UserUid, Guid customerUID);
    }
}

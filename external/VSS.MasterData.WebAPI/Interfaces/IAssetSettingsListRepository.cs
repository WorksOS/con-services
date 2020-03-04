using ClientModel.AssetSettings.Request;
using DbModel.AssetSettings;
using DbModel.Cache;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
	public interface IAssetSettingsListRepository
	{
		Task<Tuple<int, IList<AssetSettingsListDto>>> FetchEssentialAssets(AssetSettingsListRequestDto request);
		Task<IEnumerable<string>> FetchValidAssetUIds(List<string> assetUids, AssetSettingsListRequestDto request);
		//Task<Tuple<int, IEnumerable<string>>> FetchDeviceTypes(AssetSettingsListRequestDto request);
		Task<Tuple<int, IEnumerable<DeviceTypeDto>>> FetchDeviceTypesByAssetUID(AssetDeviceTypeRequest request);
	}
}

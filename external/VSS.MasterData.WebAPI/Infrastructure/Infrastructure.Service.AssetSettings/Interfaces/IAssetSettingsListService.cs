using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Response;
using ClientModel.AssetSettings.Response.AssetTargets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Service.AssetSettings.Interfaces
{
	public interface IAssetSettingsListService
    {
        Task<AssetSettingsListResponse> FetchEssentialAssets(AssetSettingsListRequest request);
        Task<IList<DeviceType>> FetchDeviceTypes(AssetDeviceTypeRequest request);
    }
}

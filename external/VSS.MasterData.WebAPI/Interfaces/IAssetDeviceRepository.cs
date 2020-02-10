using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.WebAPI.DbModel;

namespace Interfaces
{
    public interface IAssetDeviceRepository
    {
        Task<IEnumerable<AssetDeviceDto>> Fetch(string assetUIDs);
        Task<AssetDeviceDto> FetchWithDeviceUID(string deviceUID);
        Task<IEnumerable<AssetDeviceDto>> FetchByAssetUIDAndDeviceType(List<string> assetUIDs, string deviceType);
    }
}

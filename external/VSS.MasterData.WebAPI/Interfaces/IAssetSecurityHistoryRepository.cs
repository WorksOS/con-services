using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModel;
using DbModel.DeviceConfig;

namespace Interfaces
{
    public interface IAssetSecurityHistoryRepository
    {
        Task<AssetSecurityHistoryDto> InsertSecurityMode(AssetSecurityHistoryDto assetSecurityHistoryDto);
        Task<AssetSecurityHistoryDto> InsertSecurityStatus(AssetSecurityHistoryDto assetSecurityHistoryDto);
        Task<AssetSecurityHistoryDto> UpdateSecurityMode(AssetSecurityHistoryDto assetSecurityHistoryDto);
        Task<AssetSecurityHistoryDto> UpdateSecurityStatus(AssetSecurityHistoryDto assetSecurityHistoryDto);
        Task<AssetSecurityHistoryDto> Fetch(AssetSecurityHistoryDto assetSecurityHistoryDto);
    }
}
